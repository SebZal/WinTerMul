using WinTerMul.Common;

namespace WinTerMul
{
    internal class TerminalFactory : ITerminalFactory
    {
        private readonly PipeFactory _pipeFactory;

        public TerminalFactory(PipeFactory pipeFactory)
        {
            _pipeFactory = pipeFactory ?? throw new System.ArgumentNullException(nameof(pipeFactory));
        }

        public ITerminal CreateTerminal()
        {
            return Terminal.Create(_pipeFactory);
        }
    }
}
