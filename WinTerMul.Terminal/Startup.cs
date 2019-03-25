using System.Collections.Generic;

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

            services.AddSingleton(new Dictionary<PipeType, Pipe>
            {
                [PipeType.Output] = Pipe.Connect(_outputPipeId),
                [PipeType.Input] = Pipe.Connect(_inputPipeId)
            });
            services.AddSingleton<PipeStore>(x => type => x.GetRequiredService<Dictionary<PipeType, Pipe>>()[type]);

            services.AddSingleton<OutputHandler>();
        }
    }
}
