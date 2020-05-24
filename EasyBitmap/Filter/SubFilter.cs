using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.Filter
{
    public class SubFilter : IFilter
    {
        public StatusReporter StatusReporter { get; set; }

        public byte[] Defilter(ulong width, ulong height, byte[] data)
        {
            int lenght = data.Length;
            byte[] output = new byte[lenght];

            byte left = 0;
            for (int i = 0; i < lenght; i++)
            {
                left = (byte)(data[i] + left);
                output[i] = left;
                StatusReporter?.OnNext((lenght, i));
            }

            return output;
        }

        public byte[] Filter(ulong width, ulong height, byte[] data)
        {
            int lenght = data.Length;
            byte[] output = new byte[lenght];

            byte left = 0;
            for (int i = 0; i < lenght; i++)
            {
                output[i] = (byte)(data[i] - left);
                left = data[i];
                StatusReporter?.OnNext((lenght, i));
            }

            return output;
        }
    }
}
