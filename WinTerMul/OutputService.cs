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

        public OutputService(TerminalContainer terminalContainer, IKernel32Api kernel32Api)
        {
            _terminalContainer = terminalContainer;
            _kernel32Api = kernel32Api;
        }

        public async Task HandleOutputAsync()
        {
            var tasks = new List<Task>();
            short offset = 0;

            foreach (var terminal in _terminalContainer.GetTerminals())
            {
                tasks.Add(terminal.Out.ReadAsync().ContinueWith((transferableTask, state) =>
                {
                    var localOffset = (short)((object[])state)[0];
                    var localTerminal = (Terminal)((object[])state)[1];

                    var outputData = (OutputData)transferableTask.Result;
                    if (outputData != null)
                    {
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
                    }
                }, new object[] { offset, terminal }));

                offset += terminal.Width;
            }

            await Task.WhenAll(tasks).ContinueWith(_ => UpdateCursor());
        }

        private void UpdateCursor()
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
    }
}
