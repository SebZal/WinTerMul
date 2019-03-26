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
                serviceProvider.GetService<OutputService>().StartOutputHandlingThread();

                var terminalContainer = serviceProvider.GetService<TerminalContainer>();
                var resizeService = serviceProvider.GetService<ResizeService>();
                var inputService = serviceProvider.GetService<InputService>();

                while (true)
                {
                    Thread.Sleep(10);

                    var terminals = terminalContainer.GetTerminals();
                    if (terminals.Count == 0)
                    {
                        break;
                    }

                    Task.WaitAll(new[]
                    {
                        resizeService.HandleResizeAsync(),
                        inputService.HandleInputAsync()
                    });
                }
            }
        }
    }
}
