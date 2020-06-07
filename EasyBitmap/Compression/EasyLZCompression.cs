using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Easy.Compression
{
    public class EasyLZCompression : ICompression
    {
        public void Compress(Stream input, Stream output)
        {
            byte[] data;

            MemoryStream dataStream;
            if(input is MemoryStream)
            {
                dataStream = input as MemoryStream;
            }
            else
            {
                dataStream = new MemoryStream();
                Task task = input.CopyToAsync(dataStream);
                task.Wait();
                dataStream.Seek(0, SeekOrigin.Begin);
            }

            data = dataStream.ToArray();
            dataStream.Close();
            dataStream.Dispose();

            byte[] compressed = new byte[EasyLZ.MaxLengthRawEncode(data.Length)];
            int length = EasyLZ.Encode(data, 2, compressed);

            output.Write(compressed, 0, length);
        }

        public void Decompress(Stream input, Stream output)
        {
            byte[] data;

            using (MemoryStream dataStream = new MemoryStream())
            {
                Task task = input.CopyToAsync(dataStream);
                task.Wait();
                dataStream.Seek(0, SeekOrigin.Begin);

                data = dataStream.ToArray();
            }

            byte[] decompressed = EasyLZ.Decode(data);

            output.Write(decompressed, 0, decompressed.Length);
        }
    }
}
