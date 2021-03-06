﻿using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using WinTerMul.Common;

namespace WinTerMul.Terminal
{
    internal class Startup
    {
        private readonly InputArguments _inputArguments;

        public Startup(InputArguments inputArguments)
        {
            _inputArguments = inputArguments ?? throw new ArgumentNullException(nameof(inputArguments));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddWinTerMulCommon();

            services.AddSingleton(_inputArguments);

            services.AddSingleton(x => x.GetRequiredService<PipeFactory>().CreateClient(_inputArguments.OutputPipeId));
            services.AddSingleton(x => x.GetRequiredService<PipeFactory>().CreateClient(_inputArguments.InputPipeId));
            services.AddSingleton<PipeStore>(x => type => x.GetServices<IPipe>().ElementAt((int)type));

            services.AddSingleton<ProcessService>();
            services.AddSingleton<OutputService>();
            services.AddSingleton<InputService>();
        }
    }
}
