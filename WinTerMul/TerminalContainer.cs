using System;
using System.Collections.Generic;

namespace WinTerMul
{
    internal class TerminalContainer : ITerminalContainer
    {
        private readonly object _lock;
        private readonly List<ITerminal> _terminals;

        private ITerminal _activeTerminal;

        public TerminalContainer(params ITerminal[] terminals)
        {
            if (terminals == null)
            {
                throw new ArgumentNullException(nameof(terminals));
            }

            _lock = new object();
            _terminals = new List<ITerminal>(terminals);
        }

        public event EventHandler<EventArgs> ActiveTerminalChanged = (_, __) => { };

        public ITerminal ActiveTerminal
        {
            get
            {
                lock (_lock)
                {
                    return _activeTerminal;
                }
            }
            private set
            {
                lock (_lock)
                {
                    ActiveTerminalChanged(this, EventArgs.Empty);
                    _activeTerminal = value;
                }
            }
        }

        public void SetNextTerminalActive()
        {
            lock (_lock)
            {
                if (_terminals.Count == 0)
                {
                    return;
                }

                var index = _terminals.IndexOf(ActiveTerminal) + 1;
                if (index >= _terminals.Count)
                {
                    index = 0;
                }
                ActiveTerminal = _terminals[index];
            }
        }

        public void SetPreviousTerminalActive()
        {
            lock (_lock)
            {
                if (_terminals.Count == 0)
                {
                    return;
                }

                var index = _terminals.IndexOf(ActiveTerminal) - 1;
                if (index < 0)
                {
                    index = _terminals.Count - 1;
                }
                ActiveTerminal = _terminals[index];
            }
        }

        public void AddTerminal(ITerminal terminal)
        {
            lock (_lock)
            {
                var activeTerminalIndex = _terminals.IndexOf(ActiveTerminal);
                ActiveTerminal = terminal;
                _terminals.Insert(++activeTerminalIndex, ActiveTerminal);
            }
        }

        public IReadOnlyCollection<ITerminal> GetTerminals()
        {
            lock (_lock)
            {
                var terminals = new List<ITerminal>();
                ITerminal previousTerminal = null;
                var activeTerminal = ActiveTerminal;

                foreach (var terminal in _terminals.ToArray())
                {
                    if (terminal.Process.HasExited)
                    {
                        if (terminal == activeTerminal)
                        {
                            activeTerminal = previousTerminal;
                        }

                        _terminals.Remove(terminal);
                        terminal.Dispose();
                    }
                    else
                    {
                        if (activeTerminal == null)
                        {
                            activeTerminal = terminal;
                        }

                        previousTerminal = terminal;

                        terminals.Add(terminal);
                    }
                }

                ActiveTerminal = activeTerminal;

                return terminals;
            }
        }

        public void Dispose()
        {
            foreach (var terminal in _terminals)
            {
                terminal.Dispose();
            }
            _terminals.Clear();

            ActiveTerminalChanged = null;
        }
    }
}
