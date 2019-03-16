namespace WinTerMul.Common
{
    public class SerializableInputRecord : ISerializable
    {
        public SerializerType SerializerType => SerializerType.Input;

        public PInvoke.Kernel32.INPUT_RECORD InputRecord { get; set; }
    }
}
