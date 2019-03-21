using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace WinTerMul.Common
{
    public sealed class Pipe : IDisposable
    {
        private const int MemorySize = ushort.MaxValue + 1;
        private const byte NoData = byte.MinValue;
        private const byte Rewind = byte.MaxValue;

        private readonly SHA1CryptoServiceProvider _sha1;
        private readonly MemoryMappedFile _memoryMappedFile;

        private MemoryMappedViewStream _stream;
        private byte[] _previousHash;

        private Pipe(string id, MemoryMappedFile memoryMappedFile)
        {
            Id = id;
            _memoryMappedFile = memoryMappedFile;
            _sha1 = new SHA1CryptoServiceProvider();
            _previousHash = new byte[_sha1.HashSize / 8];
        }

        public string Id { get; }

        private MemoryMappedViewStream Stream => _stream ?? (_stream = _memoryMappedFile.CreateViewStream());

        public static Pipe Create()
        {
            var id = Guid.NewGuid().ToString();
            var memoryMappedFile = MemoryMappedFile.CreateNew(id, MemorySize);
            return new Pipe(id, memoryMappedFile);
        }

        public static Pipe Connect(string id)
        {
            var memoryMappedFile = MemoryMappedFile.OpenExisting(id);
            return new Pipe(id, memoryMappedFile);
        }

        public void Write(ISerializable @object, bool writeOnlyIfDataHasChanged = false)
        {
            while (!TryWrite(@object, writeOnlyIfDataHasChanged))
            {
                Thread.Sleep(10);
            }
        }

        public ISerializable Read()
        {
            var initialStreamPosition = Stream.Position;

            var firstByte = Stream.ReadByte();
            if (firstByte == NoData)
            {
                Stream.Position -= sizeof(byte);
                return null;
            }
            else if (firstByte == Rewind || firstByte == -1)
            {
                Stream.Position -= sizeof(byte);
                Stream.Write(new byte[1], 0, sizeof(byte)); // Erase rewind flag.
                Stream.Position = 0;
                return null;
            }

            var dataLengthBuffer = new byte[sizeof(ushort)];
            Stream.Read(dataLengthBuffer, 0, dataLengthBuffer.Length);
            var dataLength = BitConverter.ToUInt16(dataLengthBuffer, 0);
            var data = new byte[dataLength];
            Stream.Read(data, 0, data.Length);

            // Erase data that has been read.
            var nullData = new byte[Stream.Position - initialStreamPosition];
            Stream.Position = initialStreamPosition;
            Stream.Write(nullData, 0, nullData.Length);

            // TODO System.InvalidOperationException: 'Sequence contains no matching element
            // 1. start terminal, bash, cmatrix, resize => crash.
            var serializer = Serializers.All.Single(x => x.Type == (SerializerType)firstByte);
            return serializer.Deserialize(data);
        }

        public void Dispose()
        {
            _memoryMappedFile.Dispose();
            _stream?.Dispose();
            _sha1.Dispose();
        }

        private bool TryWrite(ISerializable @object, bool writeOnlyIfDataHasChanged)
        {
            var serializer = Serializers.All.Single(x => x.Type == @object.SerializerType);
            var data = serializer.Serialize(@object);

            if (writeOnlyIfDataHasChanged && !HasDataChanged(data))
            {
                return true;
            }

            var buffer = new byte[sizeof(byte) + sizeof(ushort) + data.Length];
            if (buffer.Length + sizeof(byte) >= MemorySize)
            {
                throw new InvalidOperationException("Data could not be written, it is exceeding available memory size.");
            }
            else if (Stream.Position + buffer.Length + sizeof(byte) >= MemorySize)
            {
                Stream.Write(new[] { Rewind }, 0, sizeof(byte));
                Stream.Position = 0;

                // Since this method must run again with same data, generate a new hash.
                _previousHash = _sha1.ComputeHash(Guid.NewGuid().ToByteArray()); // TODO find a better way to handle this

                return false;
            }

            // Check if stream is clean.
            var initialStreamPosition = Stream.Position;
            if (Stream.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                //TODO
                File.WriteAllText(@"C:\temp\wintermul.log", "ERROR");
                throw new Exception();
            }
            Stream.Position = initialStreamPosition;
            if (buffer.Any(x => x != 0))
            {
                // TODO
                //Console.WriteLine($"BUSY_{buffer.Count(x => x != 0)}_BUSY");

                // Since this method must run again with same data, generate a new hash.
                _previousHash = _sha1.ComputeHash(Guid.NewGuid().ToByteArray()); // TODO find a better way to handle this

                return false;
            }

            buffer[0] = (byte)serializer.Type;
            Array.Copy(BitConverter.GetBytes((ushort)data.Length), 0, buffer, sizeof(byte), sizeof(ushort));
            Array.Copy(data, 0, buffer, sizeof(byte) + sizeof(ushort), data.Length);

            // Write first byte last, so the stream is not read before everything is written.
            Stream.Position = initialStreamPosition + sizeof(byte);
            Stream.Write(buffer, 1, buffer.Length - 1);
            Stream.Position = initialStreamPosition;
            Stream.Write(buffer, 0, 1);
            Stream.Position = initialStreamPosition + buffer.Length;

            return true;
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
    }
}
