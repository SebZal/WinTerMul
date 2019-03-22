using System.IO;
using System.IO.Compression;
using System.Text;

using Newtonsoft.Json;

namespace WinTerMul.Common
{
    internal static class Serializer
    {
        public static byte[] Serialize(ITransferable transferable)
        {
            var json = JsonConvert.SerializeObject(transferable, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            var data = Encoding.UTF8.GetBytes(json);
            var output = new MemoryStream();
            using (var deflateStream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                deflateStream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static ITransferable Deserialize(byte[] data)
        {
            var input = new MemoryStream(data);
            using (var deflateStream = new DeflateStream(input, CompressionMode.Decompress))
            {
                var output = new MemoryStream();
                deflateStream.CopyTo(output);
                var decompressed = output.ToArray();
                var json = Encoding.UTF8.GetString(decompressed);
                return JsonConvert.DeserializeObject<ITransferable>(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
            }
        }
    }
}
