using Microsoft.Extensions.Logging;

namespace WinTerMul.Common
{
    public class PipeFactory
    {
        private readonly ILogger _logger;

        public PipeFactory(ILogger logger)
        {
            _logger = logger;
        }

        public IPipe CreateServer()
        {
            return Pipe.Create(_logger);
        }

        public IPipe CreateClient(string id)
        {
            return Pipe.Connect(id, _logger);
        }
    }
}
