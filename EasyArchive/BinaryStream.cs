using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace Easy
{
    public class BinaryStream : Stream
    {
        private Stream _stream;
        private bool _leaveOpen;

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position { get => _stream.Position; set => _stream.Position = value; }

        public BinaryStream(Stream stream, bool leaveOpen)
        {
            _stream = stream;
            _leaveOpen = leaveOpen;
        }

        public byte[] ReadBytes(int count)
        {
            byte[] buffer = new byte[count];
            Read(buffer, 0, count);
            return buffer;
        }

        public byte ReadByte()
        {
            return ReadBytes(1)[0];
        }

        public short ReadShort()
        {
            return LittleEndianConverter.ToShort(ReadBytes(2));
        }

        public ushort ReadUshort()
        {
            return LittleEndianConverter.ToUshort(ReadBytes(2));
        }

        public int ReadInt()
        {
            return LittleEndianConverter.ToInt(ReadBytes(4));
        }

        public uint ReadUint()
        {
            return LittleEndianConverter.ToUint(ReadBytes(4));
        }

        public long ReadLong()
        {
            return LittleEndianConverter.ToLong(ReadBytes(8));
        }

        public ulong ReadUlong()
        {
            return LittleEndianConverter.ToUlong(ReadBytes(8));
        }

        public void Write(byte value)
        {
            WriteByte(value);
        }

        public void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        public void Write(short value)
        {
            byte[] data = LittleEndianConverter.GetBytes(value);
            Write(data);
        }

        public void Write(ushort value)
        {
            byte[] data = LittleEndianConverter.GetBytes(value);
            Write(data);
        }

        public void Write(int value)
        {
            byte[] data = LittleEndianConverter.GetBytes(value);
            Write(data);
        }

        public void Write(uint value)
        {
            byte[] data = LittleEndianConverter.GetBytes(value);
            Write(data);
        }

        public void Write(long value)
        {
            byte[] data = LittleEndianConverter.GetBytes(value);
            Write(data);
        }

        public void Write(ulong value)
        {
            byte[] data = LittleEndianConverter.GetBytes(value);
            Write(data);
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            if (!_leaveOpen)
            {
                _stream.Close();
            }
        }
    }
}
