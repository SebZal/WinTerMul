using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace WinTerMul
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var services = new ServiceCollection();
            new Startup().ConfigureServices(services);
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var terminalContainer = serviceProvider.GetService<TerminalContainer>();
                var resizeService = serviceProvider.GetService<ResizeService>();
                var inputService = serviceProvider.GetService<InputService>();
                var outputService = serviceProvider.GetService<OutputService>();

                var inputTask = Task.CompletedTask;
                var resizeTask = Task.CompletedTask;
                var outputTask = Task.CompletedTask;

                while (true)
                {
                    Thread.Sleep(10);

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
    }
}
