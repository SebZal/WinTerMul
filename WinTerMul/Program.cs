using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

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

            var server = new TcpListener(IPAddress.Any, 43213);
            server.Start();

            var handle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_OUTPUT_HANDLE);

            using (var client = server.AcceptTcpClient())
            {
                using (var networkStream = client.GetStream())
                {
                    while (true)
                    {
                        if (!networkStream.CanRead || !networkStream.DataAvailable)
                        {
                            continue;
                        }

                        var terminalData = Serializer.Deserialize(networkStream);

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
