using System.Diagnostics;

using PInvoke;

using WinTerMul.Common;

using static WinTerMul.Common.NativeMethods;

namespace WinTerMul
{
    internal class Terminal
    {
        public Process Process { get; private set; }
        public Pipe Out { get; private set; }
        public Pipe In { get; private set; }

        public COORD CursorPosition { get; set; }
        public CONSOLE_CURSOR_INFO CursorInfo { get; set; }

        public static Terminal Create()
        {
            var terminal = new Terminal
            {
                Out = Pipe.Create(), // TODO make sure to dispose pipes
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
    }
}
