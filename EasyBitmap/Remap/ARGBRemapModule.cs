using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Easy.Remap
{
    public class ARGBRemapModule : IRemapModule
    {
        public int ColorSize { get; } = 4;

        public static readonly ARGBRemapModule Instance = new ARGBRemapModule();

        private ARGBRemapModule()
        {

        }

        public Color GetColor(Span<byte> span)
        {
            if (span.Length < ColorSize)
                throw new ArgumentException("Span is very tiny", nameof(span));
            return Color.FromArgb(span[0], span[1], span[2], span[3]);
        }

        public void Remap(ref Color color, Stream dst)
        {
            dst.WriteByte(color.A);
            dst.WriteByte(color.R);
            dst.WriteByte(color.G);
            dst.WriteByte(color.B);
        }
    }
}
