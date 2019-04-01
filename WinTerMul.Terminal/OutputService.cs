using System;
using System.Collections.Generic;
using System.Threading;
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

        private int _delay;
        private DateTime _lastSpeedUpTime;
        private CancellationTokenSource _cancellationTokenSource;
        private CharInfo[] _previousBuffer;
        private Coord _previousBufferSize;

        public OutputService(
            IKernel32Api kernel32Api,
            PipeStore pipeStore,
            ProcessService processService)
        {
            _kernel32Api = kernel32Api ?? throw new ArgumentNullException(nameof(kernel32Api));
            _outputPipe = pipeStore(PipeType.Output) ?? throw new ArgumentNullException(nameof(pipeStore));
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
        }

        public void SpeedUpPolling()
        {
            _delay = 50;
            _cancellationTokenSource?.Cancel();
            _lastSpeedUpTime = DateTime.Now;
        }

        public void SlowDownPolling()
        {
            _delay = 1000;
        }

        public async Task HandleOutputAsync()
        {
            var bufferInfo = _kernel32Api.GetConsoleScreenBufferInfo();
            ClearPreviousBufferIfNecessary(bufferInfo.Size);
            var outputData = GetOutputData(bufferInfo);
            var isOutputChanged = await _outputPipe.WriteAsync(outputData, true, _processService.CancellationToken);
            await Sleep(isOutputChanged);
        }

        public void Dispose()
        {
            _outputPipe.Dispose();
        }

        private OutputData GetOutputData(ConsoleScreenBufferInfo bufferInfo)
        {
            var outputData = new OutputData
            {
                BufferSize = bufferInfo.Size,
                BufferCoord = new Coord(),
                WriteRegion = bufferInfo.Window,
                CursorPosition = bufferInfo.CursorPosition
            };

            var buffer = _kernel32Api.ReadConsoleOutput(
                outputData.BufferSize,
                outputData.BufferCoord,
                outputData.WriteRegion);

            if (_previousBuffer == null)
            {
                outputData.Buffer = buffer;
            }
            else
            {
                outputData.BufferDiff = CreateBufferDiff(buffer);
            }
            _previousBuffer = buffer;

            outputData.CursorInfo = _kernel32Api.GetConsoleCursorInfo();

            return outputData;
        }

        private IEnumerable<BufferDiffElement> CreateBufferDiff(CharInfo[] buffer)
        {
            var bufferDiff = new List<BufferDiffElement>();
            var length = Math.Min(buffer.Length, _previousBuffer.Length);
            for (var i = 0; i < length; i++)
            {
                if (buffer[i].Attributes != _previousBuffer[i].Attributes ||
                    buffer[i].Char.AsciiChar != _previousBuffer[i].Char.AsciiChar ||
                    buffer[i].Char.UnicodeChar != _previousBuffer[i].Char.UnicodeChar)
                {
                    bufferDiff.Add(new BufferDiffElement { Index = i, CharInfo = buffer[i] });
                }
            }
            return bufferDiff;
        }

        private void ClearPreviousBufferIfNecessary(Coord bufferSize)
        {
            if (bufferSize.X != _previousBufferSize.X || bufferSize.Y != _previousBufferSize.Y)
            {
                _previousBufferSize = bufferSize;
                _previousBuffer = null;
            }
        }

        private async Task Sleep(bool isOutputChanged)
        {
            if (isOutputChanged)
            {
                SpeedUpPolling();
            }
            else if (DateTime.Now - _lastSpeedUpTime > TimeSpan.FromMilliseconds(1000))
            {
                SlowDownPolling();
            }

            _cancellationTokenSource = new CancellationTokenSource();
            await Task.Delay(_delay, _cancellationTokenSource.Token);
        }
    }
}
