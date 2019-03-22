namespace WinTerMul.Common
{
    public class TransferableInputRecord : ITransferable
    {
        public DataType DataType => DataType.Input;

        public PInvoke.Kernel32.INPUT_RECORD InputRecord { get; set; }
    }
}
