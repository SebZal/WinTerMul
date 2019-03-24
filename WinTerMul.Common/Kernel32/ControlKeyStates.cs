using System;

namespace WinTerMul.Common.Kernel32
{
    [Flags]
    public enum ControlKeyStates
    {
        RightAltPressed = 1,
        LeftAltPressed = 2,
        RightCtrlPressed = 4,
        LeftCtrlPressed = 8,
        ShiftPressed = 16,
        NumlockOn = 32,
        ScrolllockOn = 64,
        CapslockOn = 128,
        EnhancedKey = 256
    }
}
