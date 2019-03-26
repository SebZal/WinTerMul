using System;
using System.Threading;

namespace WinTerMul.Common.Kernel32
{
    internal class Kernel32Api : IKernel32Api
    {
        private IntPtr _outputHandle;
        private IntPtr _inputHandle;

        public Kernel32Api()
        {
            _inputHandle = NativeMethods.GetStdHandle(StdHandle.StdInputHandle);
            _outputHandle = NativeMethods.GetStdHandle(StdHandle.StdOutputHandle);
        }

        public CharInfo[] ReadConsoleOutput(Coord bufferSize, Coord bufferCoord, SmallRect readRegion)
        {
            var buffer = new CharInfo[bufferSize.X * bufferSize.Y];

            if (!NativeMethods.ReadConsoleOutput(_outputHandle, buffer, bufferSize, bufferCoord, ref readRegion))
            {
                // TODO handle error
                // TODO throw exception and handle every place this is used
            }

            return buffer;
        }

        public void WriteConsoleOutput(CharInfo[] buffer, Coord bufferSize, Coord bufferCoord, SmallRect writeRegion)
        {
            if (!NativeMethods.WriteConsoleOutput(_outputHandle, buffer, bufferSize, bufferCoord, ref writeRegion))
            {
                // TODO handle error
                // TODO throw exception and handle every place this is used
            }
        }

        public ConsoleScreenBufferInfo GetConsoleScreenBufferInfo()
        {
            if (!NativeMethods.GetConsoleScreenBufferInfo(_outputHandle, out var consoleScreenBufferInfo))
            {
                // TODO handle error
                // TODO throw exception and handle every place this is used
            }

            return consoleScreenBufferInfo;
        }

        public ConsoleCursorInfo GetConsoleCursorInfo()
        {
            if (!NativeMethods.GetConsoleCursorInfo(_outputHandle, out var consoleCursorInfo))
            {
                // TODO handle error
                // TODO throw exception and handle every place this is used
            }

            return consoleCursorInfo;
        }

        public void SetConsoleCursorInfo(ConsoleCursorInfo consoleCursorInfo)
        {
            if (!NativeMethods.SetConsoleCursorInfo(_outputHandle, ref consoleCursorInfo))
            {
                // TODO handle error
                // TODO throw exception and handle every place this is used
            }
        }

        public void SetConsoleCursorPosition(Coord cursorPosition)
        {
            if (!NativeMethods.SetConsoleCursorPosition(_outputHandle, cursorPosition))
            {
                // TODO handle error
                // TODO throw exception and handle every place this is used
            }
        }

        public InputRecord ReadConsoleInput()
        {
            if (!NativeMethods.GetNumberOfConsoleInputEvents(_inputHandle, out var numberOfEvents))
            {
                // TODO handle error
                // TODO throw exception and handle every place this is used
            }

            if (numberOfEvents == 0)
            {
                return new InputRecord();
            }

            if (!NativeMethods.ReadConsoleInput(_inputHandle, out var buffer, 1, out _))
            {
                // TODO handle error
                // TODO throw exception and handle every place this is used
            }

            return buffer;
        }

        public void WriteConsoleInput(InputRecord buffer)
        {
            if (!NativeMethods.WriteConsoleInput(_inputHandle, new[] { buffer }, 1, out _))
            {
                // TODO handle error
                // TODO throw exception and handle every place this is used
            }
        }

        public void SetConsoleWindowInfo(bool absolute, SmallRect consoleWindow)
        {
            if (!NativeMethods.SetConsoleWindowInfo(_outputHandle, absolute, ref consoleWindow))
            {
                // TODO handle error
                // TODO throw exception and handle every place this is used
            }
        }

        public void SetConsoleScreenBufferSize(Coord size)
        {
            if (!NativeMethods.SetConsoleScreenBufferSize(_outputHandle, size))
            {
                // TODO handle error
                // TODO throw exception and handle every place this is used
            }
        }

        public void FreeConsole()
        {
            if (!NativeMethods.FreeConsole())
            {
                // TODO handle error
                // TODO throw exception and handle every place this is used
            }
        }

        public void AttachConsole(int processId)
        {
            var timesFailed = 0;
            while (!NativeMethods.AttachConsole(processId))
            {
                if (++timesFailed > 10)
                {
                    // TODO handle error
                    // TODO throw exception and handle every place this is used
                }
                Thread.Sleep(10);
            }

            _inputHandle = NativeMethods.GetStdHandle(StdHandle.StdInputHandle);
            _outputHandle = NativeMethods.GetStdHandle(StdHandle.StdOutputHandle);
        }
    }
}
