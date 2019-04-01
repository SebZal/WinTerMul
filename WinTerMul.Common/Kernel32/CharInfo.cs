using System.Runtime.InteropServices;

namespace WinTerMul.Common.Kernel32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CharInfo
    {
        public CharInfoEncoding Char;
        public CharacterAttributesFlags Attributes;
    }
}
