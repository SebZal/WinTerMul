﻿using System;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

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
                    //// TODO don't use tab for switching terminal
                    //if (lpBuffer.Event.KeyEvent.wVirtualKeyCode == 9 && !wasTabLastKey)
                    //{
                    //    wasTabLastKey = true;
                    //    activeTerminalIndex = ++activeTerminalIndex % terminals.Length;
                    //    activeTerminal = terminals[activeTerminalIndex];
                    //    Console.Beep(); // TODO remove this
                    //    continue;
                    //}
                    //else
                    //{
                    //    wasTabLastKey = false;
                    //}

                    //// TODO temporary close function
                    //if (lpBuffer.Event.KeyEvent.wVirtualKeyCode == 0x1B) // ESC
                    //{
                    //    var killBuffer = new byte[8];
                    //    Array.Copy(BitConverter.GetBytes(-1), killBuffer, 4);
                    //    for (var i = 0; i < terminals.Length; i++)
                    //    {
                    //        using (var stream = terminals[i].In.CreateViewStream())
                    //        {
                    //            stream.Write(killBuffer, 0, killBuffer.Length);
                    //        }
                    //    }
                    //    break;
                    //}

                    //var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(lpBuffer));
                    //var buffer = new byte[data.Length + 2 * sizeof(int)];
                    //Array.Copy(BitConverter.GetBytes(++counter), buffer, 4);
                    //Array.Copy(BitConverter.GetBytes(data.Length), 0, buffer, 4, 4);
                    //Array.Copy(data, 0, buffer, 8, data.Length);
                    //using (var stream = activeTerminal.In.CreateViewStream())
                    //{
                    //    stream.Write(buffer, 0, buffer.Length);
                    //}
                }
            }
        }
    }
}
