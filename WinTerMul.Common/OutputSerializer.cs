﻿using System;

namespace WinTerMul.Common
{
    internal class OutputSerializer : ISerializer
    {
        public SerializerType Type => SerializerType.Output;

        public byte[] Serialize(TerminalData terminalData)
        {
            var data = new byte[sizeof(short) * 6 + terminalData.lpBuffer.Length * (sizeof(ushort) + sizeof(char))];

            var index = 0;
            Array.Copy(BitConverter.GetBytes(terminalData.dwBufferSize.X), 0, data, index, sizeof(short));
            index += sizeof(short);

            Array.Copy(BitConverter.GetBytes(terminalData.dwBufferSize.Y), 0, data, index, sizeof(short));
            index += sizeof(short);

            Array.Copy(BitConverter.GetBytes(terminalData.lpWriteRegion.Left), 0, data, index, sizeof(short));
            index += sizeof(short);

            Array.Copy(BitConverter.GetBytes(terminalData.lpWriteRegion.Right), 0, data, index, sizeof(short));
            index += sizeof(short);

            Array.Copy(BitConverter.GetBytes(terminalData.lpWriteRegion.Top), 0, data, index, sizeof(short));
            index += sizeof(short);

            Array.Copy(BitConverter.GetBytes(terminalData.lpWriteRegion.Bottom), 0, data, index, sizeof(short));
            index += sizeof(short);

            for (var i = 0; i < terminalData.lpBuffer.Length; i++)
            {
                var attributes = BitConverter.GetBytes((ushort)terminalData.lpBuffer[i].Attributes);
                Array.Copy(attributes, 0, data, index, sizeof(ushort));
                index += 2;

                var unicodeChar = BitConverter.GetBytes(terminalData.lpBuffer[i].Char.UnicodeChar);
                Array.Copy(unicodeChar, 0, data, index, sizeof(char));
                index += 2;
            }

            return data;
        }

        public TerminalData Deserialize(byte[] data)
        {
            var terminalData = new TerminalData
            {
                dwBufferCoord = new PInvoke.COORD { X = 0, Y = 0 },
                dwBufferSize = new PInvoke.COORD
                {
                    X = BitConverter.ToInt16(data, 0),
                    Y = BitConverter.ToInt16(data, sizeof(short))
                }
            };

            terminalData.lpWriteRegion = new PInvoke.SMALL_RECT();
            var index = sizeof(short) * 2;
            terminalData.lpWriteRegion.Left = BitConverter.ToInt16(data, index);
            index += sizeof(short);
            terminalData.lpWriteRegion.Right = BitConverter.ToInt16(data, index);
            index += sizeof(short);
            terminalData.lpWriteRegion.Top = BitConverter.ToInt16(data, index);
            index += sizeof(short);
            terminalData.lpWriteRegion.Bottom = BitConverter.ToInt16(data, index);
            index += sizeof(short);

            var charInfoLength = (data.Length - index) / (sizeof(ushort) + sizeof(char));
            terminalData.lpBuffer = new PInvoke.Kernel32.CHAR_INFO[charInfoLength];
            for (var i = 0; i < terminalData.lpBuffer.Length; i++)
            {
                terminalData.lpBuffer[i].Attributes = (PInvoke.Kernel32.CharacterAttributesFlags)BitConverter.ToUInt16(data, index);
                index += sizeof(ushort);

                terminalData.lpBuffer[i].Char = new PInvoke.Kernel32.CHAR_INFO_ENCODING
                {
                    UnicodeChar = BitConverter.ToChar(data, index)
                };
                index += sizeof(char);
            }

            return terminalData;
        }

        byte[] ISerializer.Serialize(ISerializable @object)
        {
            return Serialize((TerminalData)@object);
        }

        ISerializable ISerializer.Deserialize(byte[] data)
        {
            return Deserialize(data);
        }
    }
}
