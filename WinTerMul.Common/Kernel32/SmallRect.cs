using System.Runtime.InteropServices;

namespace WinTerMul.Common.Kernel32
{
    [StructLayout(LayoutKind.Sequential)] // TODO add this to all structs?
	public struct SmallRect
	{
		public short Left;
		public short Top;
		public short Right;
		public short Bottom;
	}
}
