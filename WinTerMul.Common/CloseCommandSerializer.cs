namespace WinTerMul.Common
{
    internal class CloseCommandSerializer : ISerializer 
    {
        public SerializerType Type => SerializerType.CloseCommand;

        public byte[] Serialize(CloseCommand closeCommand)
        {
            return new byte[0];
        }

        public CloseCommand Deserialize(byte[] data)
        {
            return new CloseCommand();
        }

        byte[] ISerializer.Serialize(ISerializable @object)
        {
            return Serialize((CloseCommand)@object);
        }

        ISerializable ISerializer.Deserialize(byte[] data)
        {
            return Deserialize(data);
        }
    }
}
