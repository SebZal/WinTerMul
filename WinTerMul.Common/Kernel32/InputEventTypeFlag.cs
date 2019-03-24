using System;

namespace WinTerMul.Common.Kernel32
{
    [Flags]
    public enum InputEventTypeFlag : short
    {
        KeyEvent = 1,
        MouseEvent = 2,
        WindowBufferSizeEvent = 4,
        MenuEvent = 8,
        FocusEvent = 16
    }
}
