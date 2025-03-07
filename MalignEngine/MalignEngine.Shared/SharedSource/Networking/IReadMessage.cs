﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MalignEngine
{
    public interface IReadMessage
    {
        bool ReadBoolean();
        void ReadPadBits();
        byte ReadByte();
        byte PeekByte();
        UInt16 ReadUInt16();
        Int16 ReadInt16();
        UInt32 ReadUInt32();
        Int32 ReadInt32();
        UInt64 ReadUInt64();
        Int64 ReadInt64();
        Single ReadSingle();
        Double ReadDouble();
        UInt32 ReadVariableUInt32();
        String ReadString();
        int ReadRangedInteger(int min, int max);
        Single ReadRangedSingle(Single min, Single max, int bitCount);
        byte[] ReadBytes(int numberOfBytes);

        int BitPosition { get; set; }
        int BytePosition { get; }
        byte[] Buffer { get; }
        int LengthBits { get; set; }
        int LengthBytes { get; }

        public NetworkConnection? Sender { get; set; }
    }
}
