using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Easy
{
    /// <summary>
    /// Provides the RLE codec for any integer data type.
    /// </summary>
    /// <typeparam name="T">The data's type. Must be an integer type or an ArgumentException will be thrown</typeparam>
    public class RLECodec
    {
        /// <summary>
        /// RLE-Encodes a data set.
        /// </summary>
        /// <param name="data">The data to encode</param>
        /// <returns>Encoded data</returns>
        public void Encode(byte[] data, Stream output)
        {
            byte marker = LessUsed(data);
            output.WriteByte(marker);

            byte firstRunValue = data[0];
            ulong runLength = 1;
            for (int i = 1; i < data.Length; i++)
            {
                byte currentValue = data[i];
                if (currentValue == firstRunValue)
                {
                    runLength++;
                }
                else
                {
                    MakeRun(firstRunValue, runLength, output, marker);

                    firstRunValue = currentValue;
                    runLength = 1;
                }
            }

            MakeRun(firstRunValue, runLength, output, marker);
        }

        private static byte LessUsed(byte[] data)
        {
            ulong[] bytes = new ulong[256];

            for (int i = 0; i < 256; i++)
            {
                bytes[i] = 0ul;
            }

            foreach (byte b in data)
            {
                bytes[b]++;
            }

            byte current = 0;
            ulong count = byte.MaxValue + 1;

            for (int i = 0; i < 256 && count != 0; i++)
            {
                if (bytes[i] < count)
                {
                    current = (byte)i;
                    count = bytes[i];
                }
            }

            return current;
        }

        private static bool ReadNext(Stream input, ref int aux, ref byte data)
        {
            bool value = (aux = input.ReadByte()) != -1;
            if (value)
                data = (byte)aux;
            return value;
        }

        /// <summary>
        /// Decodes RLE-encoded data
        /// </summary>
        /// <param name="data">RLE-encoded data</param>
        /// <returns>The original data</returns>
        public void Decode(Stream data, Stream output)
        {
            byte marker = 0;
            int aux = 0;
            ReadNext(data, ref aux, ref marker);
            byte value = 0;
            byte lenght = 0;
            while (ReadNext(data, ref aux, ref value))
            {
                if (value != marker)
                {
                    output.WriteByte(value);
                }
                else if (ReadNext(data, ref aux, ref lenght))
                {
                    if (lenght == marker)
                    {
                        output.WriteByte(marker);
                    }
                    else if (ReadNext(data, ref aux, ref value))
                    {
                        if (lenght == byte.MaxValue)
                            lenght = marker;

                        for (int i = 0; i < lenght; i++)
                        {
                            output.WriteByte(value);
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Data is not valid.");
                    }
                }
                else
                {
                    throw new ArgumentException("Data is not valid.");
                }
            }
        }

        private void MakeRun(byte value, ulong length, Stream stream, in byte marker)
        {
            while (length > 0)
            {
                if (length == 1)
                {
                    if (value == marker)
                        stream.WriteByte(marker);

                    stream.WriteByte(value);
                    length = 0;
                }
                else if ((length <= 3) && (value != marker))
                {
                    for (ulong i = 0; i < length; i++)
                        stream.WriteByte(value);
                    length = 0;
                }
                else
                {
                    byte size = (byte)Math.Min(length, byte.MaxValue - 1);
                    stream.WriteByte(marker);
                    stream.WriteByte(size == marker ? byte.MaxValue : size);
                    stream.WriteByte(value);
                    length -= size;
                }
            }
        }
    }
}
