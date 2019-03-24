using System;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class InputHandler : IDisposable
    {
        private readonly TerminalContainer _terminalContainer;
        private readonly IntPtr _inputHandle;

        private bool _wasLastKeyCtrlS;

        public InputHandler(TerminalContainer terminalContainer, IntPtr inputHandle)
        {
            _terminalContainer = terminalContainer;
            _inputHandle = inputHandle;
        }

        public void HandleInput()
        {
            PInvoke.Kernel32.ReadConsoleInput(_inputHandle, out var lpBuffer, 1, out var n);
            if (lpBuffer.EventType == PInvoke.Kernel32.InputEventTypeFlag.KEY_EVENT)
            {
                if (_wasLastKeyCtrlS && lpBuffer.Event.KeyEvent.uChar.UnicodeChar != '')
                {
                    _wasLastKeyCtrlS = false;

                    switch (lpBuffer.Event.KeyEvent.uChar.UnicodeChar)
                    {
                        case 's':
                        case '':
                        case '\0':
                        case '\u000f':
                            _wasLastKeyCtrlS = true;
                            break;
                        case 'l':
                            _terminalContainer.SetNextTerminalActive();
                            break;
                        case 'h':
                            _terminalContainer.SetPreviousTerminalActive();
                            break;
                        case 'v':
                            _terminalContainer.AddTerminal(Terminal.Create());
                            break;
                        //case 'h': // TODO horizontal split
                        default:
                            break;
                    }

                    return;
                }

                if (lpBuffer.Event.KeyEvent.uChar.UnicodeChar == '') // CTRL + s
                {
                    _wasLastKeyCtrlS = true;
                    return;
                }

                try
                {
                    _terminalContainer.ActiveTerminal?.In.Write(new TransferableInputRecord { InputRecord = lpBuffer });
                }
                catch (ObjectDisposedException)
                {
                    // Process has exited, new active terminal will be set in next iteration.
                    return;
                }
            }
        }

        public void Dispose()
        {
            _terminalContainer?.Dispose();
        }
    }
}
