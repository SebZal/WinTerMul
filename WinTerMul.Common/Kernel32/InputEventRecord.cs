using System.Runtime.InteropServices;

namespace WinTerMul.Common.Kernel32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct InputEventRecord 
    {
        public KeyEventRecord KeyEvent;
        public MouseEventRecord MouseEvent;
        public WindowBufferSizeRecord WindowBufferSizeEvent;
        public MenuEventRecord MenuEvent;
        public FocusEventRecord FocusEvent;
    }
}
