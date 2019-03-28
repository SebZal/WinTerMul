using Microsoft.Extensions.DependencyInjection;

using WinTerMul.Common.Kernel32;
using WinTerMul.Common.Logging;

namespace WinTerMul.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWinTerMulCommon(this IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => loggingBuilder.AddFileLogger());
            services.AddSingleton<IKernel32Api, Kernel32Api>();
            return services;
        }
    }
}
