using System;
using System.Linq;

using WinTerMul.Common;

namespace WinTerMul
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // TODO close created terminals when this process is killed

            var terminals = Enumerable.Range(0, 2).Select(_ => Terminal.Create()).ToArray();

            new Renderer(terminals).StartRendererThread();

            var wasTabLastKey = false;
            var activeTerminalIndex = 0;
            var activeTerminal = terminals[activeTerminalIndex];
            var inputHandle = PInvoke.Kernel32.GetStdHandle(PInvoke.Kernel32.StdHandle.STD_INPUT_HANDLE);
            var counter = 0;
            while (true)
            {
                PInvoke.Kernel32.ReadConsoleInput(inputHandle, out var lpBuffer, 1, out var n);
                if (lpBuffer.EventType == PInvoke.Kernel32.InputEventTypeFlag.KEY_EVENT)
                {
                    if (lpBuffer.Event.KeyEvent.wVirtualKeyCode == 9 && !wasTabLastKey)
                    {
                        wasTabLastKey = true;
                        activeTerminalIndex = ++activeTerminalIndex % terminals.Length;
                        activeTerminal = terminals[activeTerminalIndex];
                        Console.Beep(); // TODO remove this
                        continue;
                    }
                    else
                    {
                        wasTabLastKey = false;
                    }

                    // TODO temporary close function
                    if (lpBuffer.Event.KeyEvent.wVirtualKeyCode == 0x1B) // ESC
                    {
                        foreach (var terminal in terminals)
                        {
                            terminal.In.Write(new CloseCommand());
                        }
                        break;
                    }

                    activeTerminal.In.Write(new SerializableInputRecord { InputRecord = lpBuffer });
                }
            }
        }
    }
}
