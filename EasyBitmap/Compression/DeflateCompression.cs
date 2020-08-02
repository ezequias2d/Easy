using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Easy.Compression
{
    public class DeflateCompression : ICompression

    {
        public void Compress(Stream input, Stream output)
        {
            using (DeflateStream compressionStream = new DeflateStream(output, CompressionLevel.Optimal, true))
            {
                input.CopyTo(compressionStream);
            }
        }

        public void Decompress(Stream input, Stream output)
        {
            using (DeflateStream decompressionStream = new DeflateStream(input, System.IO.Compression.CompressionMode.Decompress, true))
            {
                decompressionStream.CopyTo(output);
            }
        }
    }
}
