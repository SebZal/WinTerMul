using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WinTerMul.Common
{
    public class WinTerMulConfiguration
    {
        private readonly IConfiguration _configuration;

        internal WinTerMulConfiguration(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public LogLevel LogLevel => (LogLevel)Enum.Parse(typeof(LogLevel), _configuration["LogLevel"]);
    }
}
