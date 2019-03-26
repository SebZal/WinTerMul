using System;
using System.Threading.Tasks;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul.Terminal
{
    internal class OutputService : IDisposable
    {
        private readonly IKernel32Api _kernel32Api;
        private readonly ProcessService _processService;
        private readonly Pipe _outputPipe;

        public OutputService(
            IKernel32Api kernel32Api,
            PipeStore pipeStore,
            ProcessService processService)
        {
            _kernel32Api = kernel32Api ?? throw new ArgumentNullException(nameof(kernel32Api));
            _outputPipe = pipeStore(PipeType.Output) ?? throw new ArgumentNullException(nameof(pipeStore));
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
        }

        public async Task HandleOutputAsync()
        {
            var bufferInfo = _kernel32Api.GetConsoleScreenBufferInfo();

            var outputData = new OutputData
            {
                Buffer = new CharInfo[bufferInfo.Size.X * bufferInfo.Size.Y],
                BufferSize = bufferInfo.Size,
                BufferCoord = new Coord(),
                WriteRegion = bufferInfo.Window,
                CursorPosition = bufferInfo.CursorPosition
            };

            outputData.Buffer = _kernel32Api.ReadConsoleOutput(
                outputData.BufferSize,
                outputData.BufferCoord,
                outputData.WriteRegion);

            outputData.CursorInfo = _kernel32Api.GetConsoleCursorInfo();

            await _outputPipe.WriteAsync(outputData, true, _processService.CancellationToken);
        }

        public void Dispose()
        {
            _outputPipe.Dispose();
        }
    }
}
