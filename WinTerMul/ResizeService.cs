﻿using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul
{
    internal class ResizeService : IDisposable
    {
        private readonly TerminalContainer _terminalContainer;
        private readonly IKernel32Api _kernel32Api;
        private readonly SHA1CryptoServiceProvider _sha1;

        private byte[] _previousHash;

        public ResizeService(
            TerminalContainer terminalContainer,
            IKernel32Api kernel32Api)
        {
            _terminalContainer = terminalContainer;
            _kernel32Api = kernel32Api;
            _sha1 = new SHA1CryptoServiceProvider();
            _previousHash = new byte[_sha1.HashSize / 8];
        }

        public async Task HandleResizeAsync()
        {
            var terminals = _terminalContainer.GetTerminals();
            if (terminals.Count == 0)
            {
                return;
            }

            var bufferInfo = _kernel32Api.GetConsoleScreenBufferInfo();
            bufferInfo.CursorPosition = new Coord(); // Ignore cursor position
            bufferInfo.MaximumWindowSize.X = (short)(bufferInfo.MaximumWindowSize.X / terminals.Count);
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
                        terminal.Width = bufferInfo.MaximumWindowSize.X;

                        await terminal.In.WriteAsync(new ResizeCommand
                        {
                            Width = bufferInfo.MaximumWindowSize.X,
                            Height = bufferInfo.MaximumWindowSize.Y
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

        public void Dispose()
        {
            _sha1.Dispose();
            _terminalContainer.Dispose();
        }
    }
}