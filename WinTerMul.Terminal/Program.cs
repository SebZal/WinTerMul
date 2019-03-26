using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace WinTerMul.Terminal
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var inputArguments = new InputArguments(args);

            var services = new ServiceCollection();
            new Startup(inputArguments).ConfigureServices(services);
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var outputService = serviceProvider.GetService<OutputService>();
                var inputService = serviceProvider.GetService<InputService>();
                var processService = serviceProvider.GetService<ProcessService>();

                processService.StartNewTerminal();

                var outputTask = Task.CompletedTask;
                var inputTask = Task.CompletedTask;
                while (!processService.ShouldClose()) // TODO use event based system instead of polling
                {
                    Thread.Sleep(10);

                    if (outputTask.IsCompleted)
                    {
                        outputTask = outputService.HandleOutputAsync();
                    }

                    if (inputTask.IsCompleted)
                    {
                        inputTask = inputService.HandleInputAsync();
                    }

                    Task.WaitAny(new[] { outputTask, inputTask });
                }
            }
        }
    }
}
