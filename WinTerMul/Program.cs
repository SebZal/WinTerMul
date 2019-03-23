using System;
using System.Threading;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var inputHandle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_INPUT_HANDLE);
            var outputHandle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_OUTPUT_HANDLE);

            var terminalContainer = new TerminalContainer(Terminal.Create()); // TODO dispose
            var resizeHandler = new ResizeHandler(terminalContainer, outputHandle); // TODO dispose

            new Renderer(terminalContainer).StartRendererThread();

            var wasLastKeyCtrlS = false;

            while (true)
            {
                Thread.Sleep(10);

                var terminals = terminalContainer.GetTerminals();
                if (terminals.Count == 0)
                {
                    break;
                }

                resizeHandler.CheckAndHandleResize();

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
