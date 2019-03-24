using System;
using System.Runtime.InteropServices;

namespace WinTerMul.Common.Kernel32
{
    internal static class NativeMethods
    { 
        // TODO cleanup, check for consistency
        // TODO remove unused methods if any

        [DllImport("kernel32.dll", EntryPoint = "ReadConsoleOutputW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ReadConsoleOutput(
            IntPtr hConsoleOutput,
            [Out] CharInfo[] lpBuffer,
            Coord dwBufferSize,
            Coord dwBufferCoord,
            ref SmallRect lpReadRegion);

        [DllImport("kernel32.dll", EntryPoint = "WriteConsoleOutputW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool WriteConsoleOutput(
            IntPtr hConsoleOutput,
            CharInfo[] lpBuffer,
            Coord dwBufferSize,
            Coord dwBufferCoord,
            ref SmallRect lpWriteRegion);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool WriteConsoleInput(
            IntPtr hConsoleInput,
            InputRecord[] lpBuffer,
            int nLength,
            out int lpNumberOfEventsWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleScreenBufferSize(IntPtr hConsoleOutput, Coord dwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleWindowInfo(
            IntPtr hConsoleOutput,
            bool bAbsolute,
            ref SmallRect lpConsoleWindow);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleCursorInfo(
            IntPtr hConsoleOutput,
            out ConsoleCursorInfo lpConsoleCursorInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCursorInfo(
            IntPtr hConsoleOutput,
            ref ConsoleCursorInfo lpConsoleCursorInfo);

        [DllImport("Kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)] // TODO should this be above all methods that return bool?
        public static extern bool SetConsoleCursorPosition(IntPtr hConsoleOutput, Coord dwCursorPosition);

        [DllImport("Kernel32", SetLastError = true)]
        public static extern bool GetConsoleScreenBufferInfo(
            IntPtr hConsoleOutput,
            out ConsoleScreenBufferInfo lpConsoleScreenBufferInfo);

        [DllImport("Kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadConsoleInput(
            IntPtr hConsoleInput,
            out InputRecord lpBuffer,
            int nLength,
            out int lpNumberOfEventsRead);
    }
}
