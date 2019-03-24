using System.Runtime.InteropServices;

namespace WinTerMul.Common.Kernel32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Coord
    {
        public short X;
		public short Y;
	}
}
