namespace WinTerMul.Common.Kernel32
{
    public struct ConsoleScreenBufferInfo
    {
        public Coord Size;
        public Coord CursorPosition;
        public CharacterAttributesFlags Attributes;
        public SmallRect Window;
        public Coord MaximumWindowSize;
    }
}
