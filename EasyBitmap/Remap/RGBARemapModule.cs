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

        public Color GetColor(Span<byte> span)
        {
            if (span.Length < ColorSize)
                throw new ArgumentException("Span is very tiny", nameof(span));
            return Color.FromArgb(span[3], span[0], span[1], span[2]);
        }

        public void Remap(ref Color color, Stream dst)
        {
            dst.WriteByte(color.R);
            dst.WriteByte(color.G);
            dst.WriteByte(color.B);
            dst.WriteByte(color.A);
        }
    }
}
