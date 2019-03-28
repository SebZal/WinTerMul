using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using WinTerMul.Common.Kernel32;
using WinTerMul.Common.Logging;

namespace WinTerMul.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWinTerMulCommon(this IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, false)
                .Build();

            var winTerMulConfiguration = new WinTerMulConfiguration(configuration);
            services.AddSingleton(winTerMulConfiguration);

            services.AddLogging(loggingBuilder => loggingBuilder.AddFileLogger(winTerMulConfiguration));
            services.AddSingleton<IKernel32Api, Kernel32Api>();
            return services;
        }
    }
}
