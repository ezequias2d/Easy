using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Easy.Remap
{
    public class GrayScaleRemapModule : IRemapModule
    {
        public int ColorSize { get; } = 1;

        public static readonly GrayScaleRemapModule Instance = new GrayScaleRemapModule();

        private GrayScaleRemapModule()
        {

        }

        public Color GetColor(Span<byte> span)
        {
            if (span.Length < ColorSize)
                throw new ArgumentException("Span is very tiny", nameof(span));
            return Color.FromArgb(255, span[0], span[0], span[0]);
        }

        public void Remap(ref Color color, Stream dst)
        {
            dst.WriteByte((byte)((color.R + color.G + color.B) / 3));
        }
    }
}
