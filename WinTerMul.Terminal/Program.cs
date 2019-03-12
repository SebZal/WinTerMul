using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;

namespace WinTerMul.Terminal
{
    class Program
    {
        static void Main(string[] args)
        {
            var sha1 = new SHA1CryptoServiceProvider();
            var previousHash = new byte[20];

            var process = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe")
                {
                }
            };
            process.Start();

            Thread.Sleep(500); // TODO

            PInvoke.Kernel32.FreeConsole();
            PInvoke.Kernel32.AttachConsole(process.Id);

            var handle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_OUTPUT_HANDLE);

            using (var client = new TcpClient("localhost", 43213))
            {
                while (true)
                {
                    if (!PInvoke.Kernel32.GetConsoleScreenBufferInfo(handle, out var bufferInfo))
                    {
                        // TODO error handling
                        throw new Exception();
                    }

                    var width = (short)(bufferInfo.srWindow.Right - bufferInfo.srWindow.Left);
                    var height = (short)(bufferInfo.srWindow.Bottom - bufferInfo.srWindow.Top);
                    var terminalData = new TerminalData
                    {
                        lpBuffer = new PInvoke.Kernel32.CHAR_INFO[width * height],
                        dwBufferSize = new PInvoke.COORD { X = width, Y = height },
                        dwBufferCoord = new PInvoke.COORD { X = 0, Y = 0 },
                        lpWriteRegion = bufferInfo.srWindow
                    };

                    var isSuccess = NativeMethods.ReadConsoleOutput(
                        handle,
                        terminalData.lpBuffer,
                        terminalData.dwBufferSize,
                        terminalData.dwBufferCoord,
                        ref terminalData.lpWriteRegion);

                    if (!isSuccess)
                    {
                        // TODO error handling
                        throw new Exception();
                    }

                    var networkStream = client.GetStream();

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var data = Serializer.Serialize(terminalData);
                    var hash = sha1.ComputeHash(data);

                    var isHashDifferent = false;
                    for (var i = 0; i < hash.Length; i++)
                    {
                        if (hash[i] != previousHash[i])
                        {
                            isHashDifferent = true;
                            break;
                        }
                    }

                    if (isHashDifferent)
                    {
                        previousHash = hash;
                        networkStream.Write(data, 0, data.Length);
                    }
                }
            }
        }
    }
}
