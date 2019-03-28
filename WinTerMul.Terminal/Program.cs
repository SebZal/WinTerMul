﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WinTerMul.Terminal
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ILogger logger = null;

            try
            {
                var inputArguments = new InputArguments(args);

                var services = new ServiceCollection();
                new Startup(inputArguments).ConfigureServices(services);
                using (var serviceProvider = services.BuildServiceProvider())
                {
                    logger = serviceProvider.GetRequiredService<ILogger>();

                    var outputService = serviceProvider.GetRequiredService<OutputService>();
                    var inputService = serviceProvider.GetRequiredService<InputService>();
                    var processService = serviceProvider.GetRequiredService<ProcessService>();

                    processService.StartNewTerminal();

                    var outputTask = Task.CompletedTask;
                    var inputTask = Task.CompletedTask;
                    while (!processService.ShouldClose()) // TODO use event based system instead of polling
                    {
                        Thread.Sleep(10);

                        if (outputTask.IsCompleted)
                        {
                            outputTask = outputService.HandleOutputAsync();
                        }

                        if (inputTask.IsCompleted)
                        {
                            inputTask = inputService.HandleInputAsync();
                        }

                        Task.WaitAny(outputTask, inputTask);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogCritical(ex, "WinTerMul.Terminal exited unexpectedly.");
                throw;
            }
        }
    }
}
