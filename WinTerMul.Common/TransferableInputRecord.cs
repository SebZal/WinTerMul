using WinTerMul.Common.Kernel32;

namespace WinTerMul.Common
{
    public class TransferableInputRecord : ITransferable
    {
        public DataType DataType => DataType.Input;

        public InputRecord InputRecord { get; set; }
    }
}
