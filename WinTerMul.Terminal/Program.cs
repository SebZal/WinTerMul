using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul.Terminal
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var outputPipeId = args[0];
            var inputPipeId = args[1];
            var parentProcessId = int.Parse(args[2]);

            using (var outputPipe = Pipe.Connect(outputPipeId))
            using (var inputPipe = Pipe.Connect(inputPipeId))
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo("cmd.exe")
                    {
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                process.Start();

                Thread.Sleep(500); // TODO

                var kernel32Api = new Kernel32Api();

                kernel32Api.FreeConsole();
                kernel32Api.AttachConsole(process.Id);

                // TODO restore handles

                while (!process.HasExited) // TODO use event based system instead of polling
                {
                    Thread.Sleep(10);
                    HandleOutput(kernel32Api, outputPipe);
                    HandleInput(kernel32Api, inputPipe, out var kill);

                    if (kill || IsProcessDead(parentProcessId))
                    {
                        KillAllChildProcesses(process.Id);
                        break;
                    }
                }
            }
        }

        private static void HandleOutput(IKernel32Api kernel32Api, Pipe outputPipe)
        {
            var bufferInfo = kernel32Api.GetConsoleScreenBufferInfo();

            var terminalData = new TerminalData
            {
                lpBuffer = new CharInfo[bufferInfo.Size.X * bufferInfo.Size.Y],
                dwBufferSize = bufferInfo.Size,
                dwBufferCoord = new Coord(),
                lpWriteRegion = bufferInfo.Window,
                dwCursorPosition = bufferInfo.CursorPosition
            };

            terminalData.lpBuffer = kernel32Api.ReadConsoleOutput(
                terminalData.dwBufferSize,
                terminalData.dwBufferCoord,
                terminalData.lpWriteRegion);

            terminalData.CursorInfo = kernel32Api.GetConsoleCursorInfo();

            outputPipe.Write(terminalData, true);
        }

        private static void HandleInput(IKernel32Api kernel32Api, Pipe inputPipe, out bool kill)
        {
            kill = false;

            var data = inputPipe.Read();
            if (data == null)
            {
                return;
            }
            else if (data.DataType == DataType.Input)
            {
                var buffer = ((TransferableInputRecord)data).InputRecord;
                kernel32Api.WriteConsoleInput(buffer);
            }
            else if (data.DataType == DataType.CloseCommand)
            {
                kill = true;
                return;
            }
            else if (data.DataType == DataType.ResizeCommand)
            {
                var resizeCommand = (ResizeCommand)data;

                // Resize crashes if window size exceeds buffer size.
                // Hence, temporary set window size to smallest possible,
                // update buffer size, and then set the final window size.
                var rect = new SmallRect
                {
                    Top = 0,
                    Bottom = 1,
                    Left = 0,
                    Right = 1
                };
                kernel32Api.SetConsoleWindowInfo(true, rect);
                var coord = new Coord
                {
                    X = resizeCommand.Width,
                    Y = resizeCommand.Height
                };

                kernel32Api.SetConsoleScreenBufferSize(coord);
                rect = new SmallRect
                {
                    Left = 0,
                    Right = (short)(resizeCommand.Width - 1),
                    Top = 0,
                    Bottom = (short)(resizeCommand.Height - 1)
                };

                var bufferInfo = kernel32Api.GetConsoleScreenBufferInfo();
                if (rect.Right >= bufferInfo.MaximumWindowSize.X)
                {
                    rect.Right = (short)(bufferInfo.MaximumWindowSize.X - 1);
                }
                if (rect.Bottom >= bufferInfo.MaximumWindowSize.Y)
                {
                    rect.Bottom = (short)(bufferInfo.MaximumWindowSize.Y - 1);
                }

                // TODO How to handle resize for child processes?
                // TODO E.g. start vifm, resize, close vifm. This results in wrong buffer size for console.
                kernel32Api.SetConsoleWindowInfo(true, rect);
            }
        }

        private static bool IsProcessDead(int id)
        {
            return Process.GetProcesses().All(x => x.Id != id);
        }

        private static void KillAllChildProcesses(int id)
        {
            if (id == 0)
            {
                return;
            }

            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ParentProcessID=" + id);

            var processes = searcher.Get();
            if (processes != null)
            {
                foreach (var process in processes)
                {
                    KillAllChildProcesses(Convert.ToInt32(process["ProcessID"]));
                }
            }

            try
            {
                var process = Process.GetProcessById(id);
                if (process != null)
                {
                    process.Kill();
                }
            }
            catch (ArgumentException)
            {
                // The process is not running or the identifier might be expired.
            }
        }
    }
}
