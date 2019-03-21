using System;
using System.Diagnostics;
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

            var outputPipe = Pipe.Connect(outputPipeId); // TODO make sure to dispose pipes
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
                HandleInput(inputHandle, outputHandle, ref messageCount, inputPipe, out var kill);

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

        private static void HandleInput(IntPtr inputHandle, IntPtr outputHandle, ref int messageCount, Pipe inputPipe, out bool kill)
        {
            kill = false;

            var data = inputPipe.Read();
            if (data == null)
            {
                return;
            }
            else if (data.SerializerType == SerializerType.Input)
            {
                var lpBuffer = new[] { ((SerializableInputRecord)data).InputRecord };
                NativeMethods.WriteConsoleInput(inputHandle, lpBuffer, lpBuffer.Length, out _);
            }
            else if (data.SerializerType == SerializerType.CloseCommand)
            {
                kill = true;
                return;
            }
            else if (data.SerializerType == SerializerType.ResizeCommand)
            {
                var resizeCommand = (ResizeCommand)data;

                // Resize crashes if window size exceeds buffer size.
                // Hence, temporary set window size to smallest possible,
                // update buffer size, and then set the final window size.
                var rect = new PInvoke.SMALL_RECT
                {
                    Top = 0,
                    Bottom = 1,
                    Left = 0,
                    Right = 1
                };
                if (NativeMethods.SetConsoleWindowInfo(outputHandle, true, ref rect))
                {
                    var coord = new PInvoke.COORD
                    {
                        X = resizeCommand.Width,
                        Y = resizeCommand.Height
                    };
                    if (NativeMethods.SetConsoleScreenBufferSize(outputHandle, coord))
                    {
                        rect = new PInvoke.SMALL_RECT
                        {
                            Left = 0,
                            Right = (short)(resizeCommand.Width - 2),
                            Top = 0,
                            Bottom = (short)(resizeCommand.Height - 2)
                        };
                        if (!NativeMethods.SetConsoleWindowInfo(outputHandle, true, ref rect))
                        {
                            // TODO
                            var error = PInvoke.Kernel32.GetLastError();
                            Console.WriteLine("3: " + error.ToString());
                            Console.WriteLine(JsonConvert.SerializeObject(rect, Formatting.Indented));
                            PInvoke.Kernel32.GetConsoleScreenBufferInfo(outputHandle, out var bufferInfo);
                            Console.WriteLine(JsonConvert.SerializeObject(bufferInfo, Formatting.Indented));
                        }
                    }
                    else
                    {
                        // TODO
                        var error = PInvoke.Kernel32.GetLastError();
                        Console.WriteLine("2: " + error.ToString());
                    }
                }
                else
                {
                    // TODO
                    var error = PInvoke.Kernel32.GetLastError();
                    Console.WriteLine("1: " + error.ToString());
                }
            }
        }
    }
}
