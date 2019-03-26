using WinTerMul.Common.Kernel32;

namespace WinTerMul.Common
{
    public class InputData : ITransferable
    {
        public DataType DataType => DataType.InputData;

        public InputRecord InputRecord { get; set; }
    }
}
