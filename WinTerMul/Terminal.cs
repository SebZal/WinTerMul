using System.Diagnostics;
using System.IO.MemoryMappedFiles;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class Terminal
    {
        public Process Process { get; private set; }
        public MemoryMappedFile Out { get; private set; }
        public MemoryMappedFile In { get; private set; }

        public static Terminal Create()
        {
            var terminal = new Terminal
            {
                Out = MemoryMappedFileUtility.CreateMemoryMappedFile(out var outName),
                In = MemoryMappedFileUtility.CreateMemoryMappedFile(out var inName)
            };

            terminal.Process = new Process
            {
                // TODO change path
                StartInfo = new ProcessStartInfo(@"C:\Users\zalewski\source\repos\WinTerMul\WinTerMul.Terminal\bin\Debug\net461\WinTerMul.Terminal.exe")
                {
                    Arguments = $"{outName} {inName}"
                }
            };
            terminal.Process.Start();

            return terminal;
        }
    }
}
