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
                    NativeMethods.SetConsoleScreenBufferSize(handle, new PInvoke.COORD
                    {
                        X = 1000, // TODO adjust these values dynamically
                        Y = 500
                    });

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
                            NativeMethods.WriteConsoleOutput(
                                handle,
                                terminalData.lpBuffer,
                                terminalData.dwBufferSize,
                                terminalData.dwBufferCoord,
                                ref terminalData.lpWriteRegion);

                            width = terminalData.dwBufferSize.X;
                            previousWidths[terminal] = width;
                        }
                        else
                        {
                            previousWidths.TryGetValue(terminal, out width);
                        }

                        offset += width;
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
