using System;
using System.IO;

namespace WinTerMul.Terminal
{
    public static class Serializer
    {
        public static byte[] Serialize(TerminalData terminalData)
        {
            var data = new byte[6 + terminalData.lpBuffer.Length * 4];

            Array.Copy(BitConverter.GetBytes(terminalData.dwBufferSize.X), 0, data, 0, 2);
            Array.Copy(BitConverter.GetBytes(terminalData.dwBufferSize.Y), 0, data, 2, 2);

            Array.Copy(BitConverter.GetBytes((short)terminalData.lpBuffer.Length), 0, data, 4, 2);

            var index = 6;
            for (var i = 0; i < terminalData.lpBuffer.Length; i++)
            {
                Array.Copy(BitConverter.GetBytes((ushort)terminalData.lpBuffer[i].Attributes), 0, data, index, 2);
                index += 2;

                Array.Copy(BitConverter.GetBytes(terminalData.lpBuffer[i].Char.UnicodeChar), 0, data, index, 2);
                index += 2;
            }

            return data;
        }

        public static TerminalData Deserialize(Stream stream)
        {
            var preamble = new byte[6];
            var dataRead = stream.Read(preamble, 0, preamble.Length);
            if (dataRead < preamble.Length)
            {
                // TODO error handling
                throw new Exception();
            }

            var terminalData = new TerminalData
            {
                dwBufferCoord = new PInvoke.COORD { X = 0, Y = 0 },
                dwBufferSize = new PInvoke.COORD
                {
                    X = BitConverter.ToInt16(preamble, 0),
                    Y = BitConverter.ToInt16(preamble, 2)
                }
            };

            terminalData.lpWriteRegion = new PInvoke.SMALL_RECT
            {
                Left = 0,
                Right = terminalData.dwBufferSize.X,
                Top = 0,
                Bottom = terminalData.dwBufferSize.Y
            };

            terminalData.lpBuffer = new PInvoke.Kernel32.CHAR_INFO[BitConverter.ToInt16(preamble, 4)];

            var data = new byte[terminalData.lpBuffer.Length * 4];
            dataRead = stream.Read(data, 0, data.Length);
            if (dataRead < data.Length)
            {
                // TODO error handling
                throw new Exception();
            }

            var index = 0;
            for (var i = 0; i < terminalData.lpBuffer.Length; i++)
            {
                terminalData.lpBuffer[i].Attributes = (PInvoke.Kernel32.CharacterAttributesFlags)BitConverter.ToUInt16(data, index);
                index += 2;
                terminalData.lpBuffer[i].Char = new PInvoke.Kernel32.CHAR_INFO_ENCODING
                {
                    UnicodeChar = BitConverter.ToChar(data, index)
                };
                index += 2;
            }

            return terminalData;
        }
    }
}
