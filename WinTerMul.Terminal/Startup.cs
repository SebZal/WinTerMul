using Microsoft.Extensions.DependencyInjection;

using WinTerMul.Common;

namespace WinTerMul.Terminal
{
    internal class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWinTerMulCommon();
        }
    }
}
