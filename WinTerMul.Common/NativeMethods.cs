using System;
using System.Runtime.InteropServices;

using PInvoke; // TODO remove PInvoke dependency

using static PInvoke.Kernel32;

namespace WinTerMul.Common
{
    public static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadConsoleOutput(
            IntPtr hConsoleOutput,
            [Out] CHAR_INFO[] lpBuffer,
            COORD dwBufferSize,
            COORD dwBufferCoord,
            ref SMALL_RECT lpReadRegion);

        [DllImport("kernel32.dll", EntryPoint = "WriteConsoleOutputW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool WriteConsoleOutput(
            IntPtr hConsoleOutput,
            CHAR_INFO[] lpBuffer,
            COORD dwBufferSize,
            COORD dwBufferCoord,
            ref SMALL_RECT lpWriteRegion);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool WriteConsoleInput(
            IntPtr hConsoleInput,
            INPUT_RECORD[] lpBuffer,
            int nLength,
            out int lpNumberOfEventsWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleScreenBufferSize(IntPtr hConsoleOutput, COORD dwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleWindowInfo(
            IntPtr hConsoleOutput,
            bool bAbsolute,
            ref SMALL_RECT lpConsoleWindow);
    }
}
