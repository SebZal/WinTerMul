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

            var terminalData = new TerminalData
            {
                lpBuffer = new CharInfo[bufferInfo.Size.X * bufferInfo.Size.Y],
                dwBufferSize = bufferInfo.Size,
                dwBufferCoord = new Coord(),
                lpWriteRegion = bufferInfo.Window,
                dwCursorPosition = bufferInfo.CursorPosition
            };

            terminalData.lpBuffer = _kernel32Api.ReadConsoleOutput(
                terminalData.dwBufferSize,
                terminalData.dwBufferCoord,
                terminalData.lpWriteRegion);

            terminalData.CursorInfo = _kernel32Api.GetConsoleCursorInfo();

            _outputPipe.Write(terminalData, true);
        }

        public void Dispose()
        {
            _outputPipe.Dispose();
        }
    }
}
