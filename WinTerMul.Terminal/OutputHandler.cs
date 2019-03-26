using System;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul.Terminal
{
    internal class OutputHandler : IDisposable
    {
        private readonly IKernel32Api _kernel32Api;
        private readonly Pipe _outputPipe;

        public OutputHandler(IKernel32Api kernel32Api, PipeStore pipeStore)
        {
            _kernel32Api = kernel32Api;
            _outputPipe = pipeStore(PipeType.Output);
        }

        public void HandleOutput()
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

            _outputPipe.Write(outputData, true);
        }

        public void Dispose()
        {
            _outputPipe.Dispose();
        }
    }
}
