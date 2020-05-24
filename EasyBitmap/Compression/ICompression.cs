using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Easy.Compression
{
    public interface ICompression
    {
        void Compress(Stream input, Stream output);

        void Decompress(Stream input, Stream output);
    }
}
