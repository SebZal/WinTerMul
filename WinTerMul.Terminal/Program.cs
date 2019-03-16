using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

using Newtonsoft.Json;

using WinTerMul.Common;

namespace WinTerMul.Terminal
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var outputPipeId = args[0];
            var inputPipeId = args[1];

            var outputPipe = Pipe.Connect(outputPipeId);
            var inputPipe = Pipe.Connect(inputPipeId);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe")
                {
                    WindowStyle = ProcessWindowStyle.Minimized
                }
            };
            process.Start();

            Thread.Sleep(500); // TODO

            PInvoke.Kernel32.FreeConsole();
            PInvoke.Kernel32.AttachConsole(process.Id);

            var outputHandle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_OUTPUT_HANDLE);
            var inputHandle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_INPUT_HANDLE);

            // TODO handle this in a better way
            PInvoke.Kernel32.GetConsoleScreenBufferInfo(outputHandle, out var inf);
            var rect = new PInvoke.SMALL_RECT
            {
                Top = (short)(inf.srWindow.Top + 0),
                Bottom = (short)(inf.srWindow.Bottom + 16),
                Left = inf.srWindow.Left,
                Right = (short)(inf.srWindow.Right - 13)
            };
            var r = NativeMethods.SetConsoleWindowInfo(outputHandle, true, ref rect);
            if (!r)
            {
                Console.WriteLine(PInvoke.Kernel32.GetLastError());
                Console.WriteLine("ERROR");
            }

            var messageCount = 0;

            while (!process.HasExited) // TODO use event based system instead of polling
            {
                Thread.Sleep(10);
                HandleOutput(outputHandle, outputPipe);
                HandleInput(inputHandle, ref messageCount, inputPipe, out var kill);

                if (kill)
                {
                    process.Kill(); // TODO this doesn't work if vifm is open
                    break;
                }
            }
        }

        private static void HandleOutput(IntPtr handle, Pipe outputPipe)
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

            outputPipe.Write(terminalData, true);
        }

        private static void HandleInput(IntPtr handle, ref int messageCount, Pipe inputPipe, out bool kill)
        {
            kill = false;

            //using (var viewStream = inputMmf.CreateViewStream())
            //{
            //    if (!viewStream.CanRead)
            //    {
            //        return;
            //    }

            //    var buffer = new byte[8];
            //    viewStream.Read(buffer, 0, buffer.Length);

            //    var count = BitConverter.ToInt32(buffer, 0);

            //    if (count == -1)
            //    {
            //        kill = true;
            //        return;
            //    }

            //    if (count != messageCount)
            //    {
            //        messageCount = count;

            //        var length = BitConverter.ToInt32(buffer, 4);
            //        buffer = new byte[length];
            //        viewStream.Read(buffer, 0, buffer.Length);

            //        var @string = Encoding.UTF8.GetString(buffer);
            //        var record = JsonConvert.DeserializeObject<PInvoke.Kernel32.INPUT_RECORD>(@string);

            //        NativeMethods.WriteConsoleInput(handle, new[] { record }, 1, out var n);
            //    }
            //}
        }
    }
}
