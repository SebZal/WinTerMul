using System;

namespace WinTerMul.Common
{
    internal class ResizeCommandSerializer : ISerializer
    {
        public SerializerType Type => SerializerType.ResizeCommand;

        public byte[] Serialize(ResizeCommand resizeCommand)
        {
            var buffer = new byte[sizeof(short) * 2];
            Array.Copy(BitConverter.GetBytes(resizeCommand.Width), buffer, sizeof(short));
            Array.Copy(BitConverter.GetBytes(resizeCommand.Height), 0, buffer, sizeof(short), sizeof(short));
            return buffer;
        }

        public ResizeCommand Deserialize(byte[] data)
        {
            return new ResizeCommand
            {
                Width = BitConverter.ToInt16(data, 0),
                Height = BitConverter.ToInt16(data, sizeof(short)),
            };
        }

        byte[] ISerializer.Serialize(ISerializable @object)
        {
            return Serialize((ResizeCommand)@object);
        }

        ISerializable ISerializer.Deserialize(byte[] data)
        {
            return Deserialize(data);
        }
    }
}
