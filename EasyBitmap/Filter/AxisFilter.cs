using System;
using System.Collections.Generic;

namespace Easy.Filter
{
    public class AxisFilter : IFilter
    {
        HashSet<byte> vs = new HashSet<byte>();

        public StatusReporter StatusReporter { get; set; }

        public byte[] Defilter(ulong width, ulong height, byte[] data)
        {
            byte[] outputBytes = new byte[width * height];

            ulong startWidth = 0;
            ulong startHeight = 0;

            byte direction;
            byte pre = 0;
            long dataLength = (long)data.Length;
            unsafe
            {
                fixed(byte* pData = data, pOutput = outputBytes)
                {
                    byte* pDataPosition = pData;

                    while (startWidth < width && startHeight < height)
                    {
                        direction = *pDataPosition;
                        pDataPosition++;
                        switch (direction)
                        {
                            case 1:
                                for (ulong i = startWidth + (startHeight * width); i < width + (startHeight * width); i++)
                                {
                                    pre += *pDataPosition;
                                    pDataPosition++;
                                    pOutput[i] = pre;
                                }
                                startHeight++;
                                break;
                            case 0:
                                for (ulong i = startWidth + (startHeight * width); i < startWidth + height * width; i += width)
                                {
                                    pre += *pDataPosition;
                                    pDataPosition++;
                                    pOutput[i] = pre;
                                }
                                startWidth++;
                                break;
                            default:
                                throw new Exception("Filter Error, Invalid data.");
                        }
                        StatusReporter?.OnNext((dataLength, (long)(pDataPosition - pData)));
                    }
                }
            }


            return outputBytes;
        }

        public byte[] Filter(ulong width, ulong height, byte[] data)
        {
            byte[] output = new byte[(int)Math.Max(width, height) * 2 + data.Length];
            int position;

            ulong startWidth = 0;
            ulong startHeight = 0;

            double countW;
            double countH;

            byte pre = 0;
            byte mpre = 0;

            unsafe
            {
                fixed (byte* pData = data, pOutput = output)
                {
                    byte* pOutputPosition = pOutput;
                    while (startWidth < width && startHeight < height)
                    {
                        countW = MediumRepetitionA8(data, pre, mpre, startWidth + (startHeight * width), (ulong)(width + startHeight * width), 1);
                        countH = MediumRepetitionA8(data, pre, mpre, startWidth + (startHeight * width), startWidth + (height * width), width);

                        if (countW > countH)
                        {
                            *pOutputPosition = 1;
                            pOutputPosition++;
                            for (ulong i = startWidth + (startHeight * width); i < ((startHeight + 1) * width); i++)
                            {
                                mpre = (byte)(data[i] - pre);
                                *pOutputPosition = mpre;
                                pOutputPosition++;
                                pre = data[i];
                            }
                            startHeight++;
                        }
                        else
                        {
                            *pOutputPosition = 0;
                            pOutputPosition++;
                            for (ulong i = startWidth + (startHeight * width); i < startWidth + (height * width); i += width)
                            {
                                mpre = (byte)(data[i] - pre);
                                *pOutputPosition = mpre;
                                pOutputPosition++;
                                pre = data[i];
                            }
                            startWidth++;
                        }
                        StatusReporter?.OnNext(((long)data.Length, (long)(startWidth + (startHeight * width))));
                    }
                    position = (int)(pOutputPosition - pOutput);
                }
            }


            return output.AsSpan<byte>(0, position).ToArray();
        }

        private double MediumRepetitionA8(in byte[] bytes, byte pre, byte mpre, in ulong start, in ulong end, in ulong increment)
        {
            vs.Clear();
            ulong total = 0;
            ulong count = 0;
            ulong subcount = 0;

            for (ulong i = start; i < end; i += increment)
            {
                total++;
                if (mpre == bytes[i] - pre)
                {
                    subcount++;
                }
                else
                {
                    count += subcount;
                    subcount = 0;
                }
                mpre = (byte)(bytes[i] - pre);
                vs.Add(mpre);
                pre = bytes[i];
            }

            return (count + subcount) + (total / (double)vs.Count);
        }


    }
}
