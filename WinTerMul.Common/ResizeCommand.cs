namespace WinTerMul.Common
{
    public class ResizeCommand : ISerializable
    {
        public SerializerType SerializerType => SerializerType.ResizeCommand;

        public short Width { get; set; }
        public short Height { get; set; }
    }
}
