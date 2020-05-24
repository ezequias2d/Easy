using PaintDotNet;
using System;

namespace Easy.Paint.NET.Plugin
{
    [Serializable]
    internal class EasyBitmapSaveConfigToken : SaveConfigToken
    {

        public PixelOrder PixelOrder { get; set; }

        public uint Compression { get; set; }

        public uint Filter { get; set; }

        public EasyBitmapSaveConfigToken()
        {
            Compression = 1;
            Filter = 1;
            PixelOrder = PixelOrder.ARGB;
        }

        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}
