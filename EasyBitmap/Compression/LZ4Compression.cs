using K4os.Compression.LZ4.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Easy.Compression
{
    public class LZ4Compression : ICompression
    {
        public void Compress(Stream input, Stream output)
        {
            using (LZ4EncoderStream stream = LZ4Stream.Encode(output, K4os.Compression.LZ4.LZ4Level.L06_HC, 1024, true))
            {
                input.CopyTo(stream);
            }
        }

        public void Decompress(Stream input, Stream output)
        {
            using (LZ4DecoderStream stream = LZ4Stream.Decode(input, 1024, true))
            {
                stream.CopyTo(output);
            }
        }
    }
}
