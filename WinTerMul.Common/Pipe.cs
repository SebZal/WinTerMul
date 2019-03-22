using System;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace WinTerMul.Common
{
    public sealed class Pipe : IDisposable
    {
        private const int MemorySize = 524288; // 2^19
        private const int ChunkSize = ushort.MaxValue + 1;
        private const byte NoData = byte.MinValue;

        private readonly SHA1CryptoServiceProvider _sha1;
        private readonly MemoryMappedFile _memoryMappedFile;

        private MemoryMappedViewStream _stream;
        private byte[] _previousHash;
        private int _currentChunk;

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

            var dataLengthBuffer = new byte[sizeof(ushort)];
            Stream.Read(dataLengthBuffer, 0, dataLengthBuffer.Length);
            var dataLength = BitConverter.ToUInt16(dataLengthBuffer, 0);
            var data = new byte[dataLength];
            Stream.Read(data, 0, data.Length);

            // Clear first byte in order to indicate that the content has been read.
            Stream.Position = initialStreamPosition;
            Stream.Write(new[] { NoData }, 0, 1);

            MoveStreamToNextChunk();

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
            var initialStreamPosition = Stream.Position;

            var isChunkFree = Stream.ReadByte() == NoData;
            Stream.Position = initialStreamPosition;
            if (!isChunkFree)
            {
                return false;
            }

            var serializer = Serializers.All.Single(x => x.Type == @object.SerializerType);
            var data = serializer.Serialize(@object);

            if (writeOnlyIfDataHasChanged && !HasDataChanged(data))
            {
                return true;
            }

            var buffer = new byte[sizeof(byte) + sizeof(ushort) + data.Length];
            if (buffer.Length > ChunkSize)
            {
                throw new InvalidOperationException("Data could not be written, it is exceeding chunk size.");
            }

            buffer[0] = (byte)serializer.Type;
            Array.Copy(BitConverter.GetBytes((ushort)data.Length), 0, buffer, sizeof(byte), sizeof(ushort));
            Array.Copy(data, 0, buffer, sizeof(byte) + sizeof(ushort), data.Length);

            // Write first byte last, so the chunk is not read before everything is written.
            Stream.Position = initialStreamPosition + sizeof(byte);
            Stream.Write(buffer, 1, buffer.Length - 1);
            Stream.Position = initialStreamPosition;
            Stream.Write(buffer, 0, 1);

            MoveStreamToNextChunk();

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

        private void MoveStreamToNextChunk()
        {
            var nextPosition = ++_currentChunk * ChunkSize;

            if (nextPosition >= MemorySize)
            {
                _currentChunk = nextPosition = 0;
            }

            Stream.Position = nextPosition;
        }
    }
}
