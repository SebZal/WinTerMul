using System;
using System.Collections.Generic;

namespace WinTerMul
{
    internal interface ITerminalContainer : IDisposable
    {
        event EventHandler<EventArgs> ActiveTerminalChanged;

        ITerminal ActiveTerminal { get; }

        void SetNextTerminalActive();
        void SetPreviousTerminalActive();
        void AddTerminal(ITerminal terminal);
        IReadOnlyCollection<ITerminal> GetTerminals();
    }
}
