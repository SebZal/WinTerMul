using System;
using System.Collections.Generic;
using System.Threading;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class Renderer
    {
        private readonly TerminalContainer _terminalContainer;

        public Renderer(TerminalContainer terminalContainer)
        {
            _terminalContainer = terminalContainer;
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

                            NativeMethods.WriteConsoleOutput(
                                handle,
                                terminalData.lpBuffer,
                                terminalData.dwBufferSize,
                                terminalData.dwBufferCoord,
                                ref terminalData.lpWriteRegion);

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
                        PInvoke.Kernel32.SetConsoleCursorPosition(handle, cursorPosition.Value);
                    }

                    var cursorInfo = _terminalContainer.ActiveTerminal?.CursorInfo;
                    if (cursorInfo.HasValue)
                    {
                        var ci = cursorInfo.Value;
                        NativeMethods.SetConsoleCursorInfo(handle, ref ci);
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
