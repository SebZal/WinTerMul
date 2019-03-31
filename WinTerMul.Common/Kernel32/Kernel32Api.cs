using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
                HandleError();
            }

            return buffer;
        }

        public void WriteConsoleOutput(CharInfo[] buffer, Coord bufferSize, Coord bufferCoord, SmallRect writeRegion)
        {
            if (!NativeMethods.WriteConsoleOutput(_outputHandle, buffer, bufferSize, bufferCoord, ref writeRegion))
            {
                HandleError();
            }
        }

        public ConsoleScreenBufferInfo GetConsoleScreenBufferInfo()
        {
            if (!NativeMethods.GetConsoleScreenBufferInfo(_outputHandle, out var consoleScreenBufferInfo))
            {
                HandleError();
            }

            return consoleScreenBufferInfo;
        }

        public ConsoleCursorInfo GetConsoleCursorInfo()
        {
            if (!NativeMethods.GetConsoleCursorInfo(_outputHandle, out var consoleCursorInfo))
            {
                HandleError();
            }

            return consoleCursorInfo;
        }

        public void SetConsoleCursorInfo(ConsoleCursorInfo consoleCursorInfo)
        {
            if (!NativeMethods.SetConsoleCursorInfo(_outputHandle, ref consoleCursorInfo))
            {
                HandleError();
            }
        }

        public void SetConsoleCursorPosition(Coord cursorPosition)
        {
            if (!NativeMethods.SetConsoleCursorPosition(_outputHandle, cursorPosition))
            {
                HandleError();
            }
        }

        public InputRecord ReadConsoleInput()
        {
            if (!NativeMethods.ReadConsoleInput(_inputHandle, out var buffer, 1, out _))
            {
                HandleError();
            }

            return buffer;
        }

        public void WriteConsoleInput(InputRecord buffer)
        {
            if (!NativeMethods.WriteConsoleInput(_inputHandle, new[] { buffer }, 1, out _))
            {
                HandleError();
            }
        }

        public void SetConsoleWindowInfo(bool absolute, SmallRect consoleWindow)
        {
            if (!NativeMethods.SetConsoleWindowInfo(_outputHandle, absolute, ref consoleWindow))
            {
                HandleError();
            }
        }

        public void SetConsoleScreenBufferSize(Coord size)
        {
            if (!NativeMethods.SetConsoleScreenBufferSize(_outputHandle, size))
            {
                HandleError();
            }
        }

        public void FreeConsole()
        {
            if (!NativeMethods.FreeConsole())
            {
                HandleError();
            }
        }

        public void AttachConsole(int processId)
        {
            var timesFailed = 0;
            while (!NativeMethods.AttachConsole(processId))
            {
                if (++timesFailed > 10)
                {
                    HandleError();
                }
                Thread.Sleep(10);
            }

            _inputHandle = NativeMethods.GetStdHandle(StdHandle.StdInputHandle);
            _outputHandle = NativeMethods.GetStdHandle(StdHandle.StdOutputHandle);
        }

        private void HandleError([CallerMemberName] string caller = null)
        {
            var errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode, $@"Method ""{caller}"" failed with Win32 error code {errorCode}.");
        }
    }
}
