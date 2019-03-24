using System;

namespace WinTerMul.Common.Kernel32
{
    [Flags]
    public enum StdHandle
    {
        StdErrorHandle = -12,
        StdOutputHandle = -11,
        StdInputHandle = -10
    }
}
