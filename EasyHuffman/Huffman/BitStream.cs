﻿using System;

namespace Easy.Huffman
{
    public ref struct BitStream
    {
        private Span<byte> _readSpan;
        private Span<byte> _writeSpan;

        private int readPosition;
        private int writePosition;

        private bool disposed;
        private byte code;
        private byte count;

        private byte writeCode;
        private byte writeCount;

        public BitStream(Span<byte> readSpan, Span<byte> writeSpan)
        {
            disposed = false;
            readPosition = 0;
            writePosition = 0;
            code = 0;
            count = 0;
            writeCode = 0;
            writeCount = 0;
            _readSpan = readSpan;
            _writeSpan = writeSpan;
        }

        public int WritePosition => writePosition;
        public int ReadPosition => readPosition;

        public byte Read()
        {            
            if (count == 0)
            {
                code = _readSpan[readPosition++];
                count = 8;
            } 

            return (byte)(code >> --count);
        }

        public void Write(byte bit)
        {
            writeCount++;
            writeCode = (byte)(writeCode | (bit << (8 - writeCode)));

            if (writeCount == 8)
                SendByte();
        }

        public void Flush()
        {
            if (writeCount != 0)
                SendByte();
        }

        private void SendByte()
        {
            writeCount = 0;
            _writeSpan[writePosition++] = writeCode;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                Flush();
                disposed = true;
            }
        }
    }
}
