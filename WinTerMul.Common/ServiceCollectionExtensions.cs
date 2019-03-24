using Microsoft.Extensions.DependencyInjection;

using WinTerMul.Common.Kernel32;

namespace WinTerMul.Common
{
    public static class ServiceCollectionExtensions
    {
        public static void AddWinTerMulCommon(this IServiceCollection services)
        {
            services.AddSingleton<IKernel32Api, Kernel32Api>();
        }
    }
}
