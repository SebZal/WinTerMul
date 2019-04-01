using System.Runtime.InteropServices;

namespace WinTerMul.Common.Kernel32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConsoleScreenBufferInfo
    {
        public Coord Size;
        public Coord CursorPosition;
        public CharacterAttributesFlags Attributes;
        public SmallRect Window;
        public Coord MaximumWindowSize;
    }
}
