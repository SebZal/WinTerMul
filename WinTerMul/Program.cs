using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WinTerMul
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ILogger logger = null;

            try
            {
                var services = new ServiceCollection();
                new Startup().ConfigureServices(services);
                using (var serviceProvider = services.BuildServiceProvider())
                {
                    logger = serviceProvider.GetRequiredService<ILogger>();

                    var terminalContainer = serviceProvider.GetRequiredService<TerminalContainer>();
                    var resizeService = serviceProvider.GetRequiredService<ResizeService>();
                    var inputService = serviceProvider.GetRequiredService<InputService>();
                    var outputService = serviceProvider.GetRequiredService<OutputService>();

                    var inputTask = Task.CompletedTask;
                    var resizeTask = Task.CompletedTask;
                    var outputTask = Task.CompletedTask;

                    while (true)
                    {
                        var terminals = terminalContainer.GetTerminals();
                        if (terminals.Count == 0)
                        {
                            break;
                        }

                        if (inputTask.IsCompleted)
                        {
                            inputTask = inputService.HandleInputAsync();
                        }

                        if (resizeTask.IsCompleted)
                        {
                            resizeTask = resizeService.HandleResizeAsync();
                        }

                        if (outputTask.IsCompleted)
                        {
                            outputTask = outputService.HandleOutputAsync();
                        }

                        Task.WaitAny(inputTask, resizeTask, outputTask);
                    }
                }
            }
            catch (Exception ex)
            {
                if (logger == null)
                {
                    File.AppendAllText(
                        "WinTerMul_critical_error.txt",
                        DateTime.Now + " " + ex.Message + Environment.NewLine);
                }
                else
                {
                    logger.LogCritical(ex, "WinTerMul exited unexpectedly.");
                }

                throw;
            }
        }
    }
}
