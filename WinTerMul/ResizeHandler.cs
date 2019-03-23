using System;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class ResizeHandler : IDisposable
    {
        private readonly TerminalContainer _terminalContainer;
        private readonly IntPtr _outputHandle;
        private readonly SHA1CryptoServiceProvider _sha1;

        private byte[] _previousHash;

        public ResizeHandler(
            TerminalContainer terminalContainer,
            IntPtr outputHandle)
        {
            _terminalContainer = terminalContainer;
            _outputHandle = outputHandle;
            _sha1 = new SHA1CryptoServiceProvider();
            _previousHash = new byte[_sha1.HashSize / 8];
        }

        public void CheckAndHandleResize()
        {
            var terminals = _terminalContainer.GetTerminals();

            if (PInvoke.Kernel32.GetConsoleScreenBufferInfo(_outputHandle, out var bufferInfo))
            {
                bufferInfo.dwCursorPosition = new PInvoke.COORD(); // Ignore cursor position
                bufferInfo.dwMaximumWindowSize.X = (short)(bufferInfo.dwMaximumWindowSize.X / terminals.Count);
                var hash = _sha1.ComputeHash(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(bufferInfo)));

                var isHashDifferent = false;
                for (int i = 0; i < hash.Length; i++)
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

                    foreach (var terminal in terminals)
                    {
                        try
                        {
                            terminal.In.Write(new ResizeCommand
                            {
                                Width = bufferInfo.dwMaximumWindowSize.X,
                                Height = bufferInfo.dwMaximumWindowSize.Y
                            });
                        }
                        catch (ObjectDisposedException)
                        {
                            // Process has exited, next iteration should resend correct data.
                            break;
                        }
                    }
                }
            }
            else
            {
                // TODO
                var error = PInvoke.Kernel32.GetLastError().ToString();
                Console.WriteLine(error);
            }
        }

        public void Dispose()
        {
            _sha1.Dispose();
        }
    }
}
