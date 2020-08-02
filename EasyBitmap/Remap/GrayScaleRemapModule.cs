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

        public Color GetColor(Span<byte> span, int positionX, int positionY, int width, int height)
        {
            int index = (positionX + positionY * width) * ColorSize;
            if (span.Length - index < ColorSize)
                throw new ArgumentException("Span is very tiny", nameof(span));
            span = span.Slice(index);
            return Color.FromArgb(255, span[0], span[0], span[0]);
        }

        public void Remap(IRemapModule remapModule, Span<byte> data, int positionX, int positionY, int width, int height, Stream dst)
        {
            Color color = remapModule.GetColor(data, positionX, positionY, width, height);
            dst.WriteByte((byte)((color.R + color.G + color.B) / 3));
        }
    }
}
