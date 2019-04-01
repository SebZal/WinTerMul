using System.Runtime.InteropServices;

namespace WinTerMul.Common.Kernel32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FocusEventRecord
    {
        public bool SetFocus;
    }
}
