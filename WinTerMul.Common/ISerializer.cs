namespace WinTerMul.Common
{
    public interface ISerializer
    {
        SerializerType Type { get; }

        byte[] Serialize(ISerializable @object);
        ISerializable Deserialize(byte[] data);
    }
}
