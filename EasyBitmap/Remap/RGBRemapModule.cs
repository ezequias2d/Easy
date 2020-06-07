using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Easy.Remap
{
    public class RGBRemapModule : IRemapModule
    {
        public int ColorSize { get; } = 3;
        
        public static readonly RGBRemapModule Instance = new RGBRemapModule();

        private RGBRemapModule()
        {

        }

        public Color GetColor(Span<byte> span)
        {
            if (span.Length < 3)
                throw new ArgumentException("Span is very tiny", nameof(span));
            return Color.FromArgb(255, span[0], span[1], span[2]);
        }

        public void Remap(ref Color color, Stream dst)
        {
            dst.WriteByte(color.R);
            dst.WriteByte(color.G);
            dst.WriteByte(color.B);
        }
    }
}
