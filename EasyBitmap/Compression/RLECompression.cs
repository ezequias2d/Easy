using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Easy.Compression
{
    public class RLECompression : ICompression
    {
        public void Compress(Stream input, Stream output)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                input.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                (new RLECodec()).Encode(memoryStream.ToArray(), output);
            }
        }

        public void Decompress(Stream input, Stream output)
        {
            (new RLECodec()).Decode(input, output);
        }
    }
}
