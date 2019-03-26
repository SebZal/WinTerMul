using System;
using System.Collections.Generic;
using System.Threading;

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

        public void StartOutputHandlingThread()
        {
            var renderer = new Thread(() =>
            {
                Dictionary<Terminal, short> previousWidths = new Dictionary<Terminal, short>();
                while (true) // TODO use event based system instead of polling
                {
                    Thread.Sleep(10);

                    short offset = 0;
                    foreach (var terminal in _terminalContainer.GetTerminals())
                    {
                        short width = 500;

                        OutputData outputData = null;
                        try
                        {
                            outputData = (OutputData)terminal.Out.Read();
                        }
                        catch (ObjectDisposedException)
                        {
                            // Process has exited, will be cleaned up by terminal container.
                            continue;
                        }

                        if (outputData != null)
                        {
                            var writeRegion = outputData.WriteRegion;
                            var cursorPosition = outputData.CursorPosition;

                            writeRegion.Left += offset;
                            writeRegion.Right += offset;
                            cursorPosition.X += offset;

                            _kernel32Api.WriteConsoleOutput(
                                outputData.Buffer,
                                outputData.BufferSize,
                                outputData.BufferCoord,
                                writeRegion);

                            terminal.CursorInfo = outputData.CursorInfo;
                            terminal.CursorPosition = cursorPosition;


                            width = outputData.BufferSize.X;
                            previousWidths[terminal] = width;
                        }
                        else
                        {
                            previousWidths.TryGetValue(terminal, out width);
                        }

                        offset += width;
                    }

                    UpdateCursor();
                }
            })
            {
                IsBackground = true
            };
            renderer.Start();
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
