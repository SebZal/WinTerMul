using System.Collections.Generic;
using System.Threading;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class Renderer
    {
        private readonly IEnumerable<Terminal> _terminals;

        public Renderer(IEnumerable<Terminal> terminals)
        {
            _terminals = terminals;
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
                    foreach (var terminal in _terminals)
                    {
                        short width = 500;

                        var terminalData = (TerminalData)terminal.Out.Read();
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

                    PInvoke.Kernel32.SetConsoleCursorPosition(handle, Program.ActiveTerminal.CursorPosition);
                    var cursorInfo = Program.ActiveTerminal.CursorInfo;
                    NativeMethods.SetConsoleCursorInfo(handle, ref cursorInfo);
                }
            })
            {
                IsBackground = true
            };
            renderer.Start();
        }
    }
}
