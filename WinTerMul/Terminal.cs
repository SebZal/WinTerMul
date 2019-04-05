using System;
using System.Diagnostics;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul
{
    internal class Terminal : ITerminal
    {
        private Terminal()
        {
        }

        public Process Process { get; private set; }
        public IPipe Out { get; private set; }
        public IPipe In { get; private set; }

        public Coord CursorPosition { get; set; }
        public ConsoleCursorInfo CursorInfo { get; set; }
        public short Width { get; set; }

        public static Terminal Create(PipeFactory pipeFactory)
        {
            if (pipeFactory == null)
            {
                throw new ArgumentNullException(nameof(pipeFactory));
            }

            var terminal = new Terminal
            {
                Out = pipeFactory.CreateServer(),
                In = pipeFactory.CreateServer()
            };

            terminal.Process = new Process
            {
                StartInfo = new ProcessStartInfo("WinTerMul.Terminal.exe")
                {
                    Arguments = $"{terminal.Out.Id} {terminal.In.Id} {Process.GetCurrentProcess().Id}",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            terminal.Process.Start();

            return terminal;
        }

        public void Dispose()
        {
            Out?.Dispose();
            Out = null;

            In?.Dispose();
            In = null;

            Process?.Dispose();
            Process = null;
        }
    }
}
