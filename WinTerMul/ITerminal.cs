using System;
using System.Diagnostics;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul
{
    internal interface ITerminal : IDisposable
    {
        Process Process { get; }
        IPipe Out { get; }
        IPipe In { get; }

        Coord CursorPosition { get; set; }
        ConsoleCursorInfo CursorInfo { get; set; }
        short Width { get; set; }
    }
}
