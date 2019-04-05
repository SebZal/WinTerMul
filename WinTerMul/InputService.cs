using System;
using System.Linq;
using System.Threading.Tasks;

using WinTerMul.Common;
using WinTerMul.Common.Kernel32;

namespace WinTerMul
{
    internal class InputService : IDisposable
    {
        private readonly ITerminalContainer _terminalContainer;
        private readonly IKernel32Api _kernel32Api;
        private readonly IWinTerMulConfiguration _configuration;
        private readonly ITerminalFactory _terminalFactory;
        private readonly int _prefixKeyWithoutCtrl;
        private readonly int _prefixKey;
        private readonly int[] _charactersToIgnoreAfterPrefixKey;

        private bool _wasLastKeyPrefixKey;

        public InputService(
            ITerminalContainer terminalContainer,
            IKernel32Api kernel32Api,
            IWinTerMulConfiguration configuration,
            ITerminalFactory terminalFactory)
        {
            _terminalContainer = terminalContainer ?? throw new ArgumentNullException(nameof(terminalContainer));
            _kernel32Api = kernel32Api ?? throw new ArgumentNullException(nameof(kernel32Api));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _terminalFactory = terminalFactory ?? throw new ArgumentNullException(nameof(terminalFactory));

            _prefixKeyWithoutCtrl = _configuration.PrefixKey[2];
            _prefixKey = _prefixKeyWithoutCtrl - 'a' + 1;
            _charactersToIgnoreAfterPrefixKey = new[] { _prefixKey, _prefixKeyWithoutCtrl, 0 };

            _kernel32Api.TreatControlCAsInput();
        }

        public async Task HandleInputAsync()
        {
            var inputRecord = await Task.Run(() => _kernel32Api.ReadConsoleInput());
            if (inputRecord.EventType == InputEventTypeFlag.KeyEvent)
            {
                if (await HandlePrefixKeyAsync(inputRecord))
                {
                    return;
                }

                var inputData = new InputData { InputRecord = inputRecord };
                await _terminalContainer.ActiveTerminal?.In?.WriteAsync(inputData);
            }
        }

        public void Dispose()
        {
            _terminalContainer?.Dispose();
        }

        private async Task<bool> HandlePrefixKeyAsync(InputRecord inputRecord)
        {
            if (_wasLastKeyPrefixKey)
            {
                _wasLastKeyPrefixKey = false;

                var unicodeChar = inputRecord.Event.KeyEvent.Char.UnicodeChar;
                if (_charactersToIgnoreAfterPrefixKey.Contains(unicodeChar))
                {
                    _wasLastKeyPrefixKey = true;
                }
                else if (unicodeChar == _configuration.SetNextTerminalActiveKey)
                {
                    _terminalContainer.SetNextTerminalActive();
                }
                else if (unicodeChar == _configuration.SetPreviousTerminalActive)
                {
                    _terminalContainer.SetPreviousTerminalActive();
                }
                else if (unicodeChar == _configuration.VerticalSplitKey)
                {
                    _terminalContainer.AddTerminal(_terminalFactory.CreateTerminal());
                }
                else if (unicodeChar == _configuration.ClosePaneKey)
                {
                    await _terminalContainer.ActiveTerminal?.In?.WriteAsync(new CloseCommand());
                }

                return true;
            }

            if (inputRecord.Event.KeyEvent.Char.UnicodeChar == _prefixKey)
            {
                _wasLastKeyPrefixKey = true;
                return true;
            }

            return false;
        }
    }
}
