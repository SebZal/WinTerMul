using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WinTerMul.Common.Logging
{
    internal static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AddFileLogger(
            this ILoggingBuilder loggingBuilder,
            IWinTerMulConfiguration configuration)
        {
            loggingBuilder.SetMinimumLevel(configuration.LogLevel);
            loggingBuilder.Services.AddTransient(x => x.GetRequiredService<ILoggerFactory>().CreateLogger(""));
            return loggingBuilder.AddProvider(new FileLoggerProvider(configuration));
        }
    }
}
