using System.Runtime.InteropServices;

namespace WinTerMul.Common.Kernel32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConsoleCursorInfo
    {
        public uint Size;
        public bool Visible;
    }
}
