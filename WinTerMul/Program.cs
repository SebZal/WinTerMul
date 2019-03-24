using System.Threading;

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
                serviceProvider.GetService<Renderer>().StartRendererThread();

                var terminalContainer = serviceProvider.GetService<TerminalContainer>();
                var resizeHandler = serviceProvider.GetService<ResizeHandler>();
                var inputHandler = serviceProvider.GetService<InputHandler>();

                while (true)
                {
                    Thread.Sleep(10);

                    var terminals = terminalContainer.GetTerminals();
                    if (terminals.Count == 0)
                    {
                        break;
                    }

                    resizeHandler.HandleResize();
                    inputHandler.HandleInput();
                }
            }
        }
    }
}
