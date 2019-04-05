using Microsoft.Extensions.Logging;

namespace WinTerMul.Common.Logging
{
    internal class FileLoggerProvider : ILoggerProvider
    {
        private readonly IWinTerMulConfiguration _configuration;

        public FileLoggerProvider(IWinTerMulConfiguration configuration)
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
