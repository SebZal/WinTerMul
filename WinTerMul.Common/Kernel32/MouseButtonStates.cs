using System;

namespace WinTerMul.Common.Kernel32
{
    [Flags]
    public enum MouseButtonStates
    {
        FromLeft1stButtonPressed = 1,
        RightmostButtonPressed = 2,
        FromLeft2ndButtonPressed = 4,
        FromLeft3rdButtonPressed = 8,
        FromLeft4thButtonPressed = 16
    }
}
