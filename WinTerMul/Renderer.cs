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

            NativeMethods.SetConsoleScreenBufferSize(handle, new PInvoke.COORD
            {
                X = 1000, // TODO adjust these values dynamically
                Y = 500
            });

            var renderer = new Thread(() =>
            {
                while (true) // TODO use event based system instead of polling
                {
                    Thread.Sleep(10);

                    short offset = 0;
                    foreach (var terminal in _terminals)
                    {
                        var terminalData = (TerminalData)terminal.Out.Read();
                        if (terminalData == null)
                        {
                            continue;
                        }

                        terminalData.lpWriteRegion.Left += offset;
                        terminalData.lpWriteRegion.Right += offset;

                        NativeMethods.WriteConsoleOutput(
                            handle,
                            terminalData.lpBuffer,
                            terminalData.dwBufferSize,
                            terminalData.dwBufferCoord,
                            ref terminalData.lpWriteRegion);

                        offset += terminalData.dwBufferSize.X;
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
