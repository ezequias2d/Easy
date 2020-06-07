using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Easy.Remap
{
    public class GrayScaleARemapModule : IRemapModule
    {
        public int ColorSize { get; } = 2;

        public static readonly GrayScaleARemapModule Instance = new GrayScaleARemapModule();

        private GrayScaleARemapModule()
        {

        }

        public Color GetColor(Span<byte> span)
        {
            if (span.Length < ColorSize)
                throw new ArgumentException("Span is very tiny", nameof(span));
            return Color.FromArgb(span[1], span[0], span[0], span[0]);
        }

        public void Remap(ref Color color, Stream dst)
        {
            dst.WriteByte((byte)((color.R + color.G + color.B) / 3));
            dst.WriteByte(color.A);
        }
    }
}
