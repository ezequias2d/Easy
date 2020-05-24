using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Easy
{
    internal class StreamWrapper : Stream
    {
        private Stream stream;
        private EasyArchive.EasyArchiveMode mode;
        public override bool CanRead
        {
            get
            {
                return mode != EasyArchive.EasyArchiveMode.Create;
            }
        }
        public override bool CanSeek
        {
            get
            {
                return stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return mode != EasyArchive.EasyArchiveMode.Read;
            }
        }

        public override long Length
        {
            get
            {
                return stream.Length;
            }
        }

        public override long Position 
        {
            get
            {
                return stream.Position;
            }
            set
            {
                stream.Position = value;
            }
        }

        public StreamWrapper(Stream stream, EasyArchive.EasyArchiveMode mode)
        {
            this.stream = stream;
            this.mode = mode;
        }

        public override void Flush()
        {
            if (mode != EasyArchive.EasyArchiveMode.Read)
                stream.Flush();
            else
                throw new InvalidOperationException("Flush is not supported because the stream is open in read mode.");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (mode != EasyArchive.EasyArchiveMode.Create)
                return stream.Read(buffer, offset, count);
            else
                throw new InvalidOperationException("Flush is not supported because the stream is open in create mode.");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if(mode != EasyArchive.EasyArchiveMode.Read)
                stream.SetLength(value);
            else
                throw new InvalidOperationException("Flush is not supported because the stream is open in read mode.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (mode != EasyArchive.EasyArchiveMode.Read)
                stream.Write(buffer, offset, count);
            else
                throw new InvalidOperationException("Flush is not supported because the stream is open in read mode.");
        }
    }
}
