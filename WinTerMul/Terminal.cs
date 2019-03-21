using System.Diagnostics;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class Terminal
    {
        public Process Process { get; private set; }
        public Pipe Out { get; private set; }
        public Pipe In { get; private set; }

        public static Terminal Create()
        {
            var terminal = new Terminal
            {
                Out = Pipe.Create(), // TODO make sure to dispose pipes
                In = Pipe.Create() 
            };

            terminal.Process = new Process
            {
                // TODO change path
                StartInfo = new ProcessStartInfo(@"C:\Users\zalewski\source\repos\WinTerMul\WinTerMul.Terminal\bin\Debug\net461\WinTerMul.Terminal.exe")
                {
                    Arguments = $"{terminal.Out.Id} {terminal.In.Id}"
                }
            };
            terminal.Process.Start();

            return terminal;
        }
    }
}
