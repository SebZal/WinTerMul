using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var terminals = Enumerable.Range(0, 1).Select(_ => CreateTerminal()).ToArray();

            var handle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_OUTPUT_HANDLE);

            while (true) // TODO use event based system instead of polling
            {
                Thread.Sleep(10);

                short offset = 0;
                for (var i = 0; i < terminals.Length; i++)
                {
                    var (terminal, mmf) = terminals[i];

                    using (var viewStream = mmf.CreateViewStream())
                    {
                        if (!viewStream.CanRead)
                        {
                            continue;
                        }

                        var terminalData = Serializer.Deserialize(viewStream);

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
            }
        }

        private static (Process Terminal, MemoryMappedFile MemoryMappedFile) CreateTerminal()
        {
            // TODO dispose and close process
            var mmf = MemoryMappedFileUtility.CreateMemoryMappedFile(out var mapName);
            var terminal = new Process
            {
                // TODO change path
                StartInfo = new ProcessStartInfo(@"C:\Users\zalewski\source\repos\WinTerMul\WinTerMul.Terminal\bin\Debug\net461\WinTerMul.Terminal.exe")
                {
                    Arguments = mapName
                }
            };
            terminal.Start();

            return (terminal, mmf);
        }
    }
}
