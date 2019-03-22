namespace WinTerMul.Common
{
    public class ResizeCommand : ITransferable
    {
        public DataType DataType => DataType.ResizeCommand;

        public short Width { get; set; }
        public short Height { get; set; }
    }
}
