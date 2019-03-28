using Microsoft.Extensions.Logging;

namespace WinTerMul.Common.Logging
{
    internal class FileLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger();
        }

        public void Dispose()
        {
        }
    }
}
