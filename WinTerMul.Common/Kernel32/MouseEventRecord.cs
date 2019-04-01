using System.Runtime.InteropServices;

namespace WinTerMul.Common.Kernel32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseEventRecord
    {
        public Coord MousePosition;
        public MouseButtonStates dwButtonState;
        public ControlKeyStates dwControlKeyState;
        public MouseEvents dwEventFlags;
    }
}
