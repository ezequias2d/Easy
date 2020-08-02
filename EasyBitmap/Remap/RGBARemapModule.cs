using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Easy.Remap
{
    public class RGBARemapModule : IRemapModule
    {
        public int ColorSize { get; } = 4;

        public static readonly RGBARemapModule Instance = new RGBARemapModule();

        private RGBARemapModule()
        {

        }

        public Color GetColor(Span<byte> span, int positionX, int positionY, int width, int height)
        {
            int index = (positionX + positionY * width) * ColorSize;
            if (span.Length - index < ColorSize)
                throw new ArgumentException("Span is very tiny", nameof(span));
            span = span.Slice(index);
            return Color.FromArgb(span[3], span[0], span[1], span[2]);
        }

        public void Remap(IRemapModule remapModule, Span<byte> data, int positionX, int positionY, int width, int height, Stream dst)
        {
            Color color = remapModule.GetColor(data, positionX, positionY, width, height);
            dst.WriteByte(color.R);
            dst.WriteByte(color.G);
            dst.WriteByte(color.B);
            dst.WriteByte(color.A);
        }
    }
}
