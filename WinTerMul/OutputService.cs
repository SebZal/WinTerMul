using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul
{
    internal class OutputService
    {
        private readonly TerminalContainer _terminalContainer;
        private readonly IKernel32Api _kernel32Api;
        private readonly ILogger _logger;
        private readonly Dictionary<Terminal, Task> _tasks;
        private readonly ConcurrentDictionary<Terminal, CharInfo[]> _previousBuffers;

        public OutputService(
            TerminalContainer terminalContainer,
            IKernel32Api kernel32Api,
            ILogger logger)
        {
            _terminalContainer = terminalContainer;
            _kernel32Api = kernel32Api;
            _logger = logger;
            _tasks = new Dictionary<Terminal, Task>();
            _previousBuffers = new ConcurrentDictionary<Terminal, CharInfo[]>();

            terminalContainer.ActiveTerminalChanged += TerminalContainer_ActiveTerminalChanged;
        }

        public async Task HandleOutputAsync()
        {
            short offset = 0;

            var terminals = _terminalContainer.GetTerminals().ToList();

            CleanupTasks(terminals);

            foreach (var terminal in terminals)
            {
                if (!_tasks.ContainsKey(terminal) || _tasks[terminal].IsCompleted)
                {
                    var state = new object[] { offset, terminal };
                    _tasks[terminal] = terminal.Out
                        .ReadAsync()
                        .ContinueWith(WriteConsoleOutput, state);
                }

                offset += terminal.Width;
            }

            await Task.WhenAny(_tasks.Values).ContinueWith(_ => UpdateCursor());
        }

        private void CleanupTasks(IEnumerable<Terminal> terminals)
        {
            var tasksToRemove = _tasks.Keys.Where(x => !terminals.Contains(x));
            foreach (var taskToRemvoe in tasksToRemove)
            {
                _tasks.Remove(taskToRemvoe);
            }
        }

        private void UpdateCursor()
        {
            try
            {
                var cursorPosition = _terminalContainer.ActiveTerminal?.CursorPosition;
                if (cursorPosition.HasValue)
                {
                    _kernel32Api.SetConsoleCursorPosition(cursorPosition.Value);
                }

                var cursorInfo = _terminalContainer.ActiveTerminal?.CursorInfo;
                if (cursorInfo.HasValue)
                {
                    _kernel32Api.SetConsoleCursorInfo(cursorInfo.Value);
                }
            }
            catch (Win32Exception ex)
            {
                _logger.LogWarning(ex, "Could not set cursor position.");
            }
        }

        private void WriteConsoleOutput(Task<ITransferable> transferableTask, object state)
        {
            var offset = (short)((object[])state)[0];
            var terminal = (Terminal)((object[])state)[1];

            if (transferableTask.IsFaulted)
            {
                _logger.LogWarning(
                    transferableTask.Exception,
                    "Transferable task for terminal(In: {inId}, Out: {outId}) failed.",
                    terminal.In?.Id,
                    terminal.Out?.Id);

                return;
            }

            var outputData = (OutputData)transferableTask.Result;
            var writeRegion = outputData.WriteRegion;
            var cursorPosition = outputData.CursorPosition;

            writeRegion.Left += offset;
            writeRegion.Right += offset;
            cursorPosition.X += offset;

            var buffer = GetBuffer(outputData, terminal);
            _previousBuffers[terminal] = buffer;

            _kernel32Api.WriteConsoleOutput(
                buffer,
                outputData.BufferSize,
                outputData.BufferCoord,
                writeRegion);

            terminal.CursorInfo = outputData.CursorInfo;
            terminal.CursorPosition = cursorPosition;
        }

        private CharInfo[] GetBuffer(OutputData outputData, Terminal terminal)
        {
            var buffer = outputData.Buffer;

            if (buffer == null && outputData.BufferDiff != null)
            {
                buffer = new CharInfo[outputData.BufferSize.X * outputData.BufferSize.Y];

                var length = Math.Min(buffer.Length, _previousBuffers[terminal].Length);
                Array.Copy(_previousBuffers[terminal], buffer, length);

                foreach (var diff in outputData.BufferDiff)
                {
                    buffer[diff.Index] = diff.CharInfo;
                }
            }

            return buffer;
        }

        private void TerminalContainer_ActiveTerminalChanged(object sender, EventArgs e)
        {
            UpdateCursor();
        }
    }
}
