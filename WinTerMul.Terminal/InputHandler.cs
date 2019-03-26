using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul.Terminal
{
    internal class InputHandler
    {
        private readonly Pipe _inputPipe;
        private readonly IKernel32Api _kernel32Api;

        public InputHandler(IKernel32Api kernel32Api, PipeStore pipeStore)
        {
            _kernel32Api = kernel32Api;
            _inputPipe = pipeStore(PipeType.Input);
        }

        public void HandleInput(out bool kill)
        {
            kill = false;

            var data = _inputPipe.Read();
            if (data == null)
            {
                return;
            }
            else if (data.DataType == DataType.InputData)
            {
                _kernel32Api.WriteConsoleInput(((InputData)data).InputRecord);
            }
            else if (data.DataType == DataType.ResizeCommand)
            {
                HandleResize((ResizeCommand)data);
            }
            else if (data.DataType == DataType.CloseCommand)
            {
                kill = true;
            }
        }

        private void HandleResize(ResizeCommand resizeCommand)
        {
            // Resize crashes if window size exceeds buffer size.
            // Hence, temporary set window size to smallest possible,
            // update buffer size, and then set the final window size.
            var rect = new SmallRect
            {
                Top = 0,
                Bottom = 1,
                Left = 0,
                Right = 1
            };
            _kernel32Api.SetConsoleWindowInfo(true, rect);
            var coord = new Coord
            {
                X = resizeCommand.Width,
                Y = resizeCommand.Height
            };

            _kernel32Api.SetConsoleScreenBufferSize(coord);
            rect = new SmallRect
            {
                Left = 0,
                Right = (short)(resizeCommand.Width - 1),
                Top = 0,
                Bottom = (short)(resizeCommand.Height - 1)
            };

            var bufferInfo = _kernel32Api.GetConsoleScreenBufferInfo();
            if (rect.Right >= bufferInfo.MaximumWindowSize.X)
            {
                rect.Right = (short)(bufferInfo.MaximumWindowSize.X - 1);
            }
            if (rect.Bottom >= bufferInfo.MaximumWindowSize.Y)
            {
                rect.Bottom = (short)(bufferInfo.MaximumWindowSize.Y - 1);
            }

            // TODO How to handle resize for child processes?
            // TODO E.g. start vifm, resize, close vifm. This results in wrong buffer size for console.
            _kernel32Api.SetConsoleWindowInfo(true, rect);
        }
    }
}
