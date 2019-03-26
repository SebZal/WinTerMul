using System;
using System.Diagnostics;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul
{
    internal class Terminal : IDisposable
    {
        public Process Process { get; private set; }
        public Pipe Out { get; private set; }
        public Pipe In { get; private set; }

        public Coord CursorPosition { get; set; }
        public ConsoleCursorInfo CursorInfo { get; set; }

        public short Width { get; set; } = 500;

        public static Terminal Create()
        {
            var terminal = new Terminal
            {
                Out = Pipe.Create(),
                In = Pipe.Create() 
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
            // TODO setting to null causes null reference exceptions, fix these

            Out.Dispose();
            Out = null;

            In.Dispose();
            In = null;

            Process.Dispose();
            Process = null;
        }
    }
}
