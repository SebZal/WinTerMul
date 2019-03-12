using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Threading;

using WinTerMul.Terminal;

namespace WinTerMul
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var terminal = new Process
            {
                // TODO change path
                StartInfo = new ProcessStartInfo(@"C:\Users\zalewski\source\repos\WinTerMul\WinTerMul.Terminal\bin\Debug\net461\WinTerMul.Terminal.exe")
                {
                }
            };
            terminal.Start();

            var handle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_OUTPUT_HANDLE);

            using (var mmf = MemoryMappedFile.CreateOrOpen("WinTerMul", 65536)) // TODO create helper class for this, create new GUID for file name and send this GUID to child processes.
            {
                while (true) // TODO use event based system instead of polling
                {
                    Thread.Sleep(10);

                    using (var viewStream = mmf.CreateViewStream())
                    {
                        if (!viewStream.CanRead)
                        {
                            continue;
                        }

                        var terminalData = Serializer.Deserialize(viewStream);

                        NativeMethods.WriteConsoleOutput(
                            handle,
                            terminalData.lpBuffer,
                            terminalData.dwBufferSize,
                            terminalData.dwBufferCoord,
                            ref terminalData.lpWriteRegion);
                    }
                }
            }
        }
    }
}
