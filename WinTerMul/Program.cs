using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Newtonsoft.Json;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class Program
    {
        internal static Terminal ActiveTerminal { get; private set; }
        internal static Terminal[] Terminals { get; private set; }

        private static void Main(string[] args)
        {
            Terminals = Enumerable.Range(0, 2).Select(_ => Terminal.Create()).ToArray();

            new Renderer().StartRendererThread();

            var activeTerminalIndex = 0;
            ActiveTerminal = Terminals[activeTerminalIndex];
            var inputHandle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_INPUT_HANDLE);
            var outputHandle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_OUTPUT_HANDLE);

            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                var wasLastKeyCtrlS = false;

                var previousHash = new byte[sha1.HashSize / 8];
                while (true)
                {
                    Thread.Sleep(10);

                    var numTerminals = Terminals.Length;
                    Terminals = Terminals.Where(x => !x.Process.HasExited).ToArray();
                    if (numTerminals != Terminals.Length)
                    {
                        if (Terminals.Length > 0 && !Terminals.Contains(ActiveTerminal))
                        {
                            activeTerminalIndex = 0;
                            ActiveTerminal = Terminals[activeTerminalIndex];
                        }

                        // Force resize
                        previousHash = new byte[sha1.HashSize / 8]; // TODO find a better way
                    }
                    if (Terminals.Length == 0)
                    {
                        break;
                    }

                    if (PInvoke.Kernel32.GetConsoleScreenBufferInfo(outputHandle, out var bufferInfo))
                    {
                        bufferInfo.dwCursorPosition = new PInvoke.COORD(); // Ignore cursor position
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

                            var width = (short)(bufferInfo.dwMaximumWindowSize.X / Terminals.Length);
                            var height = bufferInfo.dwMaximumWindowSize.Y;
                            foreach (var terminal in Terminals)
                            {
                                terminal.In.Write(new ResizeCommand { Width = width, Height = height });
                            }
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
                                    activeTerminalIndex = ++activeTerminalIndex % Terminals.Length;
                                    ActiveTerminal = Terminals[activeTerminalIndex];
                                    break;
                                case 'h':
                                    activeTerminalIndex = --activeTerminalIndex < 0 ? Terminals.Length - 1 : activeTerminalIndex;
                                    ActiveTerminal = Terminals[activeTerminalIndex];
                                    break;
                                case 'v':
                                    ActiveTerminal = Terminal.Create();
                                    var terminalList = Terminals.ToList();
                                    terminalList.Insert(++activeTerminalIndex, ActiveTerminal);
                                    Terminals = terminalList.ToArray();
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

                        ActiveTerminal.In.Write(new TransferableInputRecord { InputRecord = lpBuffer });
                    }
                }
            }
        }
    }
}
