using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul
{
    internal class ResizeService : IDisposable
    {
        private readonly ITerminalContainer _terminalContainer;
        private readonly IKernel32Api _kernel32Api;

        private short _previousWidth;
        private short _previousHeight;
        private (DateTime, Func<Task>)? _pendingReisze;

        public ResizeService(
            ITerminalContainer terminalContainer,
            IKernel32Api kernel32Api)
        {
            _terminalContainer = terminalContainer ?? throw new ArgumentNullException(nameof(terminalContainer));
            _kernel32Api = kernel32Api ?? throw new ArgumentNullException(nameof(kernel32Api));
        }

        public async Task HandleResizeAsync()
        {
            var terminals = _terminalContainer.GetTerminals();
            if (terminals.Count == 0)
            {
                return;
            }

            var bufferInfo = _kernel32Api.GetConsoleScreenBufferInfo();
            var width = (short)(bufferInfo.MaximumWindowSize.X / terminals.Count);
            var height = bufferInfo.MaximumWindowSize.Y;

            if (IsResizeNecessary(width, height))
            {
                _pendingReisze = (DateTime.Now, async () => await ResizeTerminals(terminals, width, height));
            }


            if (_pendingReisze.HasValue)
            {
                if (DateTime.Now - _pendingReisze.Value.Item1 > TimeSpan.FromMilliseconds(100))
                {
                    await _pendingReisze.Value.Item2();
                    _pendingReisze = null;
                }

                await Task.Delay(25);
            }
            else
            {
                await Task.Delay(500);
            }
        }

        public void Dispose()
        {
            _terminalContainer.Dispose();
        }

        private async Task ResizeTerminals(IEnumerable<ITerminal> terminals, short width, short height)
        {
            foreach (var terminal in terminals)
            {
                terminal.Width = width;

                await terminal.In?.WriteAsync(new ResizeCommand
                {
                    Width = width,
                    Height = height
                });
            }
        }

        private bool IsResizeNecessary(short width, short height)
        {
            var hasSizeChanged = false;

            if (_previousWidth != width || _previousHeight != height)
            {
                hasSizeChanged = true;
                _previousWidth = width;
                _previousHeight = height;
            }

            return hasSizeChanged;
        }
    }
}
