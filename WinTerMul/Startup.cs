using Microsoft.Extensions.DependencyInjection;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWinTerMulCommon();

            services.AddSingleton(_ => new TerminalContainer(Terminal.Create()));
            services.AddSingleton<ResizeService>();
            services.AddSingleton<InputService>();
            services.AddSingleton<OutputService>();
        }
    }
}
