using System;

namespace WinTerMul.Common.Kernel32
{
    [Flags]
    public enum MouseEvents
    {
        None = 0,
        MouseMoved = 1,
        DoubleClick = 2,
        MouseWheeled = 4,
        MouseHwheeled = 8
    }
}
