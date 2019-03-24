namespace WinTerMul.Common.Kernel32
{
    public struct MouseEventRecord
    {
        public Coord MousePosition;
        public MouseButtonStates dwButtonState;
        public ControlKeyStates dwControlKeyState;
        public MouseEvents dwEventFlags;
    }
}
