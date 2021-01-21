using PaintDotNet;
using System;

namespace Easy.Paint.NET.Plugin
{
    [Serializable]
    internal class EasyBitmapSaveConfigToken : SaveConfigToken
    {

        public PixelFormat PixelOrder { get; set; }

        public uint Compression { get; set; }

        public uint Filter { get; set; }

        public EasyBitmapSaveConfigToken()
        {
            Compression = 1;
            Filter = 1;
            PixelOrder = PixelFormat.ARGB;
        }

        public override object Clone()
        {
            return MemberwiseClone();
        }
    }
}
