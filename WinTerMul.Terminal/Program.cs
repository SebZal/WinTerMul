using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

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
                var outputService = serviceProvider.GetService<OutputService>();
                var inputService = serviceProvider.GetService<InputService>();

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

                    outputService.HandleOutput();
                    inputService.HandleInput(out var kill);

                    if (kill || IsProcessDead(parentProcessId))
                    {
                        KillAllChildProcesses(process.Id);
                        break;
                    }
                }
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
