using System.Threading;

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

                while (!processService.ShouldClose()) // TODO use event based system instead of polling
                {
                    Thread.Sleep(10);

                    outputService.HandleOutput();
                    inputService.HandleInput();
                }
            }
        }
    }
}
