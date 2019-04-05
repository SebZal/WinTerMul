using Microsoft.Extensions.Logging;

namespace WinTerMul.Common.Logging
{
    internal class FileLoggerProvider : ILoggerProvider
    {
        private readonly WinTerMulConfiguration _configuration;

        public FileLoggerProvider(WinTerMulConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_configuration);
        }

        public void Dispose()
        {
        }
    }
}
