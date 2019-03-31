using System;
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
        private readonly Dictionary<Terminal, CharInfo[]> _previousBuffers;

        public OutputService(
            TerminalContainer terminalContainer,
            IKernel32Api kernel32Api,
            ILogger logger)
        {
            _terminalContainer = terminalContainer;
            _kernel32Api = kernel32Api;
            _logger = logger;
            _tasks = new Dictionary<Terminal, Task>();
            _previousBuffers = new Dictionary<Terminal, CharInfo[]>();

            terminalContainer.ActiveTerminalChanged += TerminalContainer_ActiveTerminalChanged;
        }

        public async Task HandleOutputAsync()
        {
            short offset = 0;

            var terminals = _terminalContainer.GetTerminals().ToList();

            var tasksToRemove = _tasks.Keys.Where(x => !terminals.Contains(x));
            foreach (var taskToRemvoe in tasksToRemove)
            {
                _tasks.Remove(taskToRemvoe);
            }

            foreach (var terminal in terminals)
            {
                if (!_tasks.ContainsKey(terminal) || _tasks[terminal].IsCompleted)
                {
                    _tasks[terminal] = terminal.Out.ReadAsync().ContinueWith((transferableTask, state) =>
                    {
                        if (transferableTask.IsFaulted)
                        {
                            return;
                        }

                        var localOffset = (short)((object[])state)[0];
                        var localTerminal = (Terminal)((object[])state)[1];

                        var outputData = (OutputData)transferableTask.Result;
                        var writeRegion = outputData.WriteRegion;
                        var cursorPosition = outputData.CursorPosition;

                        writeRegion.Left += localOffset;
                        writeRegion.Right += localOffset;
                        cursorPosition.X += localOffset;

                        var buffer = GetBuffer(outputData, terminal);
                        _previousBuffers[terminal] = buffer;

                        _kernel32Api.WriteConsoleOutput(
                            buffer,
                            outputData.BufferSize,
                            outputData.BufferCoord,
                            writeRegion);

                        localTerminal.CursorInfo = outputData.CursorInfo;
                        localTerminal.CursorPosition = cursorPosition;
                    }, new object[] { offset, terminal });
                }

                offset += terminal.Width;
            }

            await Task.WhenAny(_tasks.Values).ContinueWith(_ => UpdateCursor());
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
