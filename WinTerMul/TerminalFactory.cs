using WinTerMul.Common;

namespace WinTerMul
{
    internal class TerminalFactory
    {
        private readonly PipeFactory _pipeFactory;

        public TerminalFactory(PipeFactory pipeFactory)
        {
            _pipeFactory = pipeFactory ?? throw new System.ArgumentNullException(nameof(pipeFactory));
        }

        public Terminal CreateTerminal()
        {
            return Terminal.Create(_pipeFactory);
        }
    }
}
