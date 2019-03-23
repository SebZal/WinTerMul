using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Newtonsoft.Json;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var terminalContainer = new TerminalContainer(Terminal.Create());

            new Renderer(terminalContainer).StartRendererThread();

            var inputHandle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_INPUT_HANDLE);
            var outputHandle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_OUTPUT_HANDLE);

            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                var wasLastKeyCtrlS = false;

                var previousHash = new byte[sha1.HashSize / 8];
                while (true)
                {
                    Thread.Sleep(10);

                    var terminals = terminalContainer.GetTerminals();
                    if (terminals.Count == 0)
                    {
                        break;
                    }

                    if (PInvoke.Kernel32.GetConsoleScreenBufferInfo(outputHandle, out var bufferInfo))
                    {
                        bufferInfo.dwCursorPosition = new PInvoke.COORD(); // Ignore cursor position
                        bufferInfo.dwMaximumWindowSize.X  = (short)(bufferInfo.dwMaximumWindowSize.X / terminals.Count);
                        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(bufferInfo)));

                        var isHashDifferent = false;
                        for (int i = 0; i < hash.Length; i++)
                        {
                            if (hash[i] != previousHash[i])
                            {
                                isHashDifferent = true;
                                break;
                            }
                        }

                        if (isHashDifferent)
                        {
                            previousHash = hash;

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

                            continue;
                        }
                    }
                    else
                    {
                        // TODO
                        var error = PInvoke.Kernel32.GetLastError().ToString();
                        Console.WriteLine(error);
                    }

                    PInvoke.Kernel32.ReadConsoleInput(inputHandle, out var lpBuffer, 1, out var n);
                    if (lpBuffer.EventType == PInvoke.Kernel32.InputEventTypeFlag.KEY_EVENT)
                    {
                        if (wasLastKeyCtrlS && lpBuffer.Event.KeyEvent.uChar.UnicodeChar != '')
                        {
                            wasLastKeyCtrlS = false;

                            switch (lpBuffer.Event.KeyEvent.uChar.UnicodeChar)
                            {
                                case 's':
                                case '':
                                case '\0':
                                case '\u000f':
                                    wasLastKeyCtrlS = true;
                                    break;
                                case 'l':
                                    terminalContainer.SetNextTerminalActive();
                                    break;
                                case 'h':
                                    terminalContainer.SetPreviousTerminalActive();
                                    break;
                                case 'v':
                                    terminalContainer.AddTerminal(Terminal.Create());
                                    // Force resize
                                    previousHash = new byte[sha1.HashSize / 8]; // TODO find a better way
                                    break;
                                //case 'h': // TODO horizontal split
                                default:
                                    break;
                            }

                            continue;
                        }

                        if (lpBuffer.Event.KeyEvent.uChar.UnicodeChar == '') // CTRL + s
                        {
                            wasLastKeyCtrlS = true;
                            continue;
                        }

                        try
                        {
                            terminalContainer.ActiveTerminal?.In.Write(new TransferableInputRecord { InputRecord = lpBuffer });
                        }
                        catch (ObjectDisposedException)
                        {
                            // Process has exited, new active terminal will be set in next iteration.
                            continue;
                        }
                    }
                }
            }
        }
    }
}
