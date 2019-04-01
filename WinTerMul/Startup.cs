using Microsoft.Extensions.DependencyInjection;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWinTerMulCommon();

            services.AddTransient<TerminalFactory>();

            services.AddSingleton(x =>
            {
                return new TerminalContainer(x.GetRequiredService<TerminalFactory>().CreateTerminal());
            });
            services.AddSingleton<ResizeService>();
            services.AddSingleton<InputService>();
            services.AddSingleton<OutputService>();
        }
    }
}
