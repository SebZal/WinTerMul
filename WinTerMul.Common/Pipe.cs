using System;
using System.IO.Pipes;
using System.Security.Cryptography;

namespace WinTerMul.Common
{
    public sealed class Pipe : IDisposable
    {
        private readonly SHA1CryptoServiceProvider _sha1;

        private PipeStream _stream;
        private byte[] _previousHash;

        private Pipe(string id, PipeStream stream)
        {
            Id = id;
            _stream = stream;
            _sha1 = new SHA1CryptoServiceProvider();
            _previousHash = new byte[_sha1.HashSize / 8];
        }

        public string Id { get; }

        public static Pipe Create()
        {
            var id = Guid.NewGuid().ToString();
            var stream = new NamedPipeServerStream(id);
            return new Pipe(id, stream);
        }

        public static Pipe Connect(string id)
        {
            var stream = new NamedPipeClientStream(id);
            stream.Connect();
            return new Pipe(id, stream);
        }

        public void Write(ITransferable @object, bool writeOnlyIfDataHasChanged = false)
        {
            VerifyIsConnected();

            var data = Serializer.Serialize(@object);

            if (writeOnlyIfDataHasChanged && !HasDataChanged(data))
            {
                return;
            }

            _stream.WaitForPipeDrain();

            var buffer = new byte[sizeof(ushort) + data.Length];
            Array.Copy(BitConverter.GetBytes((ushort)data.Length), 0, buffer, 0, sizeof(ushort));
            Array.Copy(data, 0, buffer, sizeof(ushort), data.Length);

            _stream.Write(buffer, 0, buffer.Length);
        }

        public ITransferable Read()
        {
            VerifyIsConnected();

            var dataLengthBuffer = new byte[sizeof(ushort)];
            _stream.Read(dataLengthBuffer, 0, dataLengthBuffer.Length);
            var dataLength = BitConverter.ToUInt16(dataLengthBuffer, 0);
            var data = new byte[dataLength];
            _stream.Read(data, 0, data.Length);

            return Serializer.Deserialize(data);
        }

        public void Dispose()
        {
            _stream.Dispose();
            _sha1.Dispose();
        }

        private bool HasDataChanged(byte[] data)
        {
            var hash = _sha1.ComputeHash(data);

            var isHashDifferent = false;
            for (var i = 0; i < hash.Length; i++)
            {
                if (hash[i] != _previousHash[i])
                {
                    isHashDifferent = true;
                    break;
                }
            }

            if (isHashDifferent)
            {
                _previousHash = hash;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void VerifyIsConnected()
        {
            if (_stream is NamedPipeServerStream s && !_stream.IsConnected)
            {
                s.WaitForConnection();
            }
        }
    }
}
