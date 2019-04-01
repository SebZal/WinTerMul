using System.Runtime.InteropServices;

namespace WinTerMul.Common.Kernel32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CharInfoEncoding
    {
        public char UnicodeChar;
        public byte AsciiChar;
    }
}
