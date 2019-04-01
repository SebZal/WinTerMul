using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul
{
    internal class ResizeService : IDisposable
    {
        private readonly TerminalContainer _terminalContainer;
        private readonly IKernel32Api _kernel32Api;

        private short _previousWidth;
        private short _previousHeight;

        public ResizeService(
            TerminalContainer terminalContainer,
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
                await ResizeTerminals(terminals, width, height);
            }

            await Task.Delay(500);
        }

        public void Dispose()
        {
            _terminalContainer.Dispose();
        }

        private async Task ResizeTerminals(IEnumerable<Terminal> terminals, short width, short height)
        {
            foreach (var terminal in terminals)
            {
                terminal.Width = width;

                await terminal.In.WriteAsync(new ResizeCommand
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
