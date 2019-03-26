using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

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

            var services = new ServiceCollection();
            new Startup(outputPipeId, inputPipeId).ConfigureServices(services);
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var inputPipe = serviceProvider.GetService<PipeStore>()(PipeType.Input);
                var outputHandler = serviceProvider.GetService<OutputHandler>();

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo("cmd.exe")
                    {
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
                };
                process.Start();

                var kernel32Api = serviceProvider.GetService<IKernel32Api>();

                kernel32Api.FreeConsole();
                kernel32Api.AttachConsole(process.Id);

                while (!process.HasExited) // TODO use event based system instead of polling
                {
                    Thread.Sleep(10);
                    outputHandler.HandleOutput();
                    HandleInput(kernel32Api, inputPipe, out var kill);

                    if (kill || IsProcessDead(parentProcessId))
                    {
                        KillAllChildProcesses(process.Id);
                        break;
                    }
                }
            }
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
