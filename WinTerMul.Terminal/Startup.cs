using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using WinTerMul.Common;

namespace WinTerMul.Terminal
{
    internal class Startup
    {
        private readonly string _outputPipeId;
        private readonly string _inputPipeId;

        public Startup(string outputPipeId, string inputPipeId)
        {
            _outputPipeId = outputPipeId;
            _inputPipeId = inputPipeId;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddWinTerMulCommon();

            services.AddSingleton(_ => Pipe.Connect(_outputPipeId));
            services.AddSingleton(_ => Pipe.Connect(_inputPipeId));
            services.AddSingleton<PipeStore>(x => type => x.GetServices<Pipe>().ElementAt((int)type));

            services.AddSingleton<OutputHandler>();
        }
    }
}
