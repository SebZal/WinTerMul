using WinTerMul.Common.Kernel32;

namespace WinTerMul.Common
{
    public class OutputData : ITransferable
    {
        public DataType DataType => DataType.OutputData;

        public CharInfo[] Buffer { get; set; }
        public Coord BufferSize { get; set; }
        public Coord BufferCoord { get; set; }
        public SmallRect WriteRegion { get; set; }
        public Coord CursorPosition { get; set; }
        public ConsoleCursorInfo CursorInfo { get; set; }
    }
}
