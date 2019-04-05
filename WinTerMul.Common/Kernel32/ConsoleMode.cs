using System;

namespace WinTerMul.Common.Kernel32
{
    [Flags]
    public enum ConsoleMode : uint
    {
        EnableProcessedInput = 0x0001,
        EnableProcessedOutput = 0x0001,
        EnableLineInput = 0x0002,
        EnableWrapAtEolOutput = 0x0002,
        EnableEchoInput = 0x0004,
        EnableVirtualTerminalProcessing = 0x0004,
        EnableWindowInput = 0x0008,
        DisableNewlineAutoReturn = 0x0008,
        EnableMouseInput = 0x0010,
        EnableLvbGridWorldwide = 0x0010,
        EnableInsertMode = 0x0020,
        EnableQuickEditmode = 0x0040,
        EnableExtendedFlags = 0x0080,
        EnableVirtualTerminalInput = 0x0200
    }
}
