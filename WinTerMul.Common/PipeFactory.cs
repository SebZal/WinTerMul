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

        public Pipe CreateServer()
        {
            return Pipe.Create(_logger);
        }

        public Pipe CreateClient(string id)
        {
            return Pipe.Connect(id, _logger);
        }
    }
}
