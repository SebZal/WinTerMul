using System;

namespace WinTerMul.Terminal
{
    internal class ResizeEventArgs : EventArgs
    {
        public ResizeEventArgs(short width, short height)
        {
            Width = width;
            Height = height;
        }

        public short Width { get; }
        public short Height { get; }
    }
}
