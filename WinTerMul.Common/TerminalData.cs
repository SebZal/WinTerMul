using WinTerMul.Common.Kernel32;

namespace WinTerMul.Common
{
    public class TerminalData : ITransferable
    {
        public DataType DataType => DataType.Output;

        // TODO rename fields, make properties
        public CharInfo[] lpBuffer;
        public Coord dwBufferSize;
        public Coord dwBufferCoord;
        public SmallRect lpWriteRegion;
        public Coord dwCursorPosition;

        public ConsoleCursorInfo CursorInfo;
    }
}
