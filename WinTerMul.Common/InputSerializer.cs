using System.Text;

using Newtonsoft.Json;

namespace WinTerMul.Common
{
    internal class InputSerializer : ISerializer
    {
        public SerializerType Type => SerializerType.Input;

        public byte[] Serialize(SerializableInputRecord inputRecord)
        {
            // TODO use a better method instead of serializing to json
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(inputRecord));
        }

        public SerializableInputRecord Deserialize(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<SerializableInputRecord>(json);
        }

        byte[] ISerializer.Serialize(ISerializable @object)
        {
            return Serialize((SerializableInputRecord)@object);
        }

        ISerializable ISerializer.Deserialize(byte[] data)
        {
            return Deserialize(data);
        }
    }
}
