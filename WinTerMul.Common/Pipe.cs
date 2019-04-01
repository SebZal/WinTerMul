using System;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace WinTerMul.Common
{
    public sealed class Pipe : IDisposable
    {
        private readonly SHA1CryptoServiceProvider _sha1;

        private PipeStream _stream;
        private readonly ILogger _logger;
        private byte[] _previousHash;

        private Pipe(string id, PipeStream stream, ILogger logger)
        {
            Id = id;
            _stream = stream;
            _logger = logger;
            _sha1 = new SHA1CryptoServiceProvider();
            _previousHash = new byte[_sha1.HashSize / 8];
        }

        public string Id { get; }

        internal static Pipe Create(ILogger logger)
        {
            var id = Guid.NewGuid().ToString();
            var stream = new NamedPipeServerStream(id);
            logger.LogInformation("Created named pipe server {id}.", id);
            return new Pipe(id, stream, logger);
        }

        internal static Pipe Connect(string id, ILogger logger)
        {
            var stream = new NamedPipeClientStream(id);
            logger.LogInformation("Connecting to named pipe server {id}.", id);
            stream.Connect();
            logger.LogInformation("Successfully connected to named pipe server {id}.", id);
            return new Pipe(id, stream, logger);
        }

        public async Task<bool> WriteAsync(
            ITransferable @object,
            bool writeOnlyIfDataHasChanged = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await VerifyIsConnectedAsync(cancellationToken);

            var data = Serializer.Serialize(@object);

            if (writeOnlyIfDataHasChanged && !HasDataChanged(data))
            {
                return false;
            }

            _stream.WaitForPipeDrain();

            var buffer = new byte[sizeof(ushort) + data.Length];
            Array.Copy(BitConverter.GetBytes((ushort)data.Length), 0, buffer, 0, sizeof(ushort));
            Array.Copy(data, 0, buffer, sizeof(ushort), data.Length);

            await _stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);

            return true;
        }

        public async Task<ITransferable> ReadAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await VerifyIsConnectedAsync(cancellationToken);

            var dataLengthBuffer = new byte[sizeof(ushort)];
            await _stream.ReadAsync(dataLengthBuffer, 0, dataLengthBuffer.Length, cancellationToken);
            var dataLength = BitConverter.ToUInt16(dataLengthBuffer, 0);
            var data = new byte[dataLength];
            await _stream.ReadAsync(data, 0, data.Length, cancellationToken);

            return Serializer.Deserialize(data);
        }

        public void Dispose()
        {
            if (_stream != null)
            {
                var isServer = _stream is NamedPipeServerStream;
                _logger.LogInformation(
                    "Closing named pipe {serverOrClient} {id}.",
                    isServer ? "server" : "client",
                    Id);

                _stream.Dispose();
                _stream = null;
            }

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

        private async Task VerifyIsConnectedAsync(CancellationToken cancellationToken)
        {
            if (_stream is NamedPipeServerStream s && !_stream.IsConnected)
            {
                _logger.LogInformation("Waiting for client to connect to named pipe server {id}.", Id);
                await s.WaitForConnectionAsync(cancellationToken);
                _logger.LogInformation("Client successfully connected to named pipe server {id}.", Id);
            }
        }
    }
}
