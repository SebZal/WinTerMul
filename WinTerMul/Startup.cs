using Microsoft.Extensions.DependencyInjection;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWinTerMulCommon();

            services.AddTransient<ITerminalFactory, TerminalFactory>();

            services.AddSingleton<ITerminalContainer>(x =>
            {
                return new TerminalContainer(x.GetRequiredService<ITerminalFactory>().CreateTerminal());
            });
            services.AddSingleton<ResizeService>();
            services.AddSingleton<InputService>();
            services.AddSingleton<OutputService>();
        }
    }
}
