using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul
{
    internal class OutputService
    {
        private readonly TerminalContainer _terminalContainer;
        private readonly IKernel32Api _kernel32Api;
        private readonly Dictionary<Terminal, Task> _tasks;

        public OutputService(TerminalContainer terminalContainer, IKernel32Api kernel32Api)
        {
            _terminalContainer = terminalContainer;
            _kernel32Api = kernel32Api;
            _tasks = new Dictionary<Terminal, Task>();

            terminalContainer.ActiveTerminalChanged += TerminalContainer_ActiveTerminalChanged;
        }

        public async Task HandleOutputAsync()
        {
            short offset = 0;

            foreach (var terminal in _terminalContainer.GetTerminals())
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

                        _kernel32Api.WriteConsoleOutput(
                            outputData.Buffer,
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

        public void UpdateCursor()
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

        private void TerminalContainer_ActiveTerminalChanged(object sender, EventArgs e)
        {
            UpdateCursor();
        }
    }
}
