using System.Diagnostics;
using System.Threading;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var mmf = MemoryMappedFileUtility.CreateMemoryMappedFile(out var mapName))
            {
                var terminal = new Process
                {
                    // TODO change path
                    StartInfo = new ProcessStartInfo(@"C:\Users\zalewski\source\repos\WinTerMul\WinTerMul.Terminal\bin\Debug\net461\WinTerMul.Terminal.exe")
                    {
                        Arguments = mapName
                    }
                };
                terminal.Start();

                var handle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_OUTPUT_HANDLE);

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
