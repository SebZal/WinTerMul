namespace WinTerMul.Common.Kernel32
{
    public interface IKernel32Api
    {
        CharInfo[] ReadConsoleOutput(Coord bufferSize, Coord bufferCoord, SmallRect readRegion);
        void WriteConsoleOutput(CharInfo[] buffer, Coord bufferSize, Coord bufferCoord, SmallRect writeRegion);
        ConsoleScreenBufferInfo GetConsoleScreenBufferInfo();
        ConsoleCursorInfo GetConsoleCursorInfo();
        void SetConsoleCursorInfo(ConsoleCursorInfo consoleCursorInfo);
        void SetConsoleCursorPosition(Coord cursorPosition);
        InputRecord ReadConsoleInput();
        void WriteConsoleInput(InputRecord buffer);
        void SetConsoleWindowInfo(bool absolute, SmallRect consoleWindow);
        void SetConsoleScreenBufferSize(Coord size);
        void FreeConsole();
        void AttachConsole(int processId);
        void TreatControlCAsInput();
    }
}
