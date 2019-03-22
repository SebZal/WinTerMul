using PInvoke;

using static PInvoke.Kernel32;

namespace WinTerMul.Common
{
    public class TerminalData : ITransferable
    {
        public DataType DataType => DataType.Output;

        public CHAR_INFO[] lpBuffer;
        public COORD dwBufferSize;
        public COORD dwBufferCoord;
        public SMALL_RECT lpWriteRegion;
        public COORD dwCursorPosition;
    }
}
