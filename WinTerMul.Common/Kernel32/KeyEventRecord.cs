namespace WinTerMul.Common.Kernel32
{
    public struct KeyEventRecord
    {
        public bool KeyDown;
        public ushort RepeatCount;
        public ushort VirtualKeyCode;
        public ushort VirtualScanCode;
        public CharInfoEncoding Char;
        public ControlKeyStates ControlKeyState;
    }
}
