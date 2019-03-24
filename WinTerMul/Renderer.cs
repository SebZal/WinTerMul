using System;
using System.Collections.Generic;
using System.Threading;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul
{
    internal class Renderer
    {
        private readonly TerminalContainer _terminalContainer;
        private readonly IKernel32Api _kernel32Api;

        public Renderer(TerminalContainer terminalContainer, IKernel32Api kernel32Api)
        {
            _terminalContainer = terminalContainer;
            _kernel32Api = kernel32Api;
        }

        public void StartRendererThread()
        {
            var handle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_OUTPUT_HANDLE);

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

                        TerminalData terminalData = null;
                        try
                        {
                            terminalData = (TerminalData)terminal.Out.Read();
                        }
                        catch (ObjectDisposedException)
                        {
                            // Process has exited, will be cleaned up by terminal container.
                            continue;
                        }

                        if (terminalData != null)
                        {
                            terminalData.lpWriteRegion.Left += offset;
                            terminalData.lpWriteRegion.Right += offset;
                            terminalData.dwCursorPosition.X += offset;

                            _kernel32Api.WriteConsoleOutput(
                                terminalData.lpBuffer,
                                terminalData.dwBufferSize,
                                terminalData.dwBufferCoord,
                                terminalData.lpWriteRegion);

                            terminal.CursorInfo = terminalData.CursorInfo;
                            terminal.CursorPosition = terminalData.dwCursorPosition;


                            width = terminalData.dwBufferSize.X;
                            previousWidths[terminal] = width;
                        }
                        else
                        {
                            previousWidths.TryGetValue(terminal, out width);
                        }

                        offset += width;
                    }

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
            })
            {
                IsBackground = true
            };
            renderer.Start();
        }
    }
}
