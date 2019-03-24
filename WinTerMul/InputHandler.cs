using System;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul
{
    internal class InputHandler : IDisposable
    {
        private readonly TerminalContainer _terminalContainer;
        private readonly IKernel32Api _kernel32Api;

        private bool _wasLastKeyCtrlS;

        public InputHandler(TerminalContainer terminalContainer, IKernel32Api kernel32Api)
        {
            _terminalContainer = terminalContainer;
            _kernel32Api = kernel32Api;
        }

        public void HandleInput()
        {
            var inputRecord = _kernel32Api.ReadConsoleInput();
            if (inputRecord.EventType == InputEventTypeFlag.KeyEvent)
            {
                if (_wasLastKeyCtrlS)
                {
                    _wasLastKeyCtrlS = false;

                    switch (inputRecord.Event.KeyEvent.Char.UnicodeChar)
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

                // TODO change to something different than CTRL+s
                if (inputRecord.Event.KeyEvent.Char.UnicodeChar == '') // CTRL + s
                {
                    _wasLastKeyCtrlS = true;
                    return;
                }

                try
                {
                    _terminalContainer.ActiveTerminal?.In.Write(new TransferableInputRecord { InputRecord = inputRecord });
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
