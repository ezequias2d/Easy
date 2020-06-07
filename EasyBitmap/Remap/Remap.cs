using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Easy.Remap
{
    public static class Remap
    {
        public static byte[] RemapColors(byte[] src, IRemapModule srcRemapModule, IRemapModule dstRemapModule)
        {
            Span<byte> srcSpan = src;

            Color color;
            using (MemoryStream dst = new MemoryStream())
            {
                while(srcSpan.Length >= srcRemapModule.ColorSize)
                {
                    color = srcRemapModule.GetColor(srcSpan);
                    dstRemapModule.Remap(ref color, dst);

                    srcSpan = srcSpan.Slice(srcRemapModule.ColorSize);
                }

                return dst.ToArray();
            }
        }

        public static byte[] RemapColors(EasyBitmap bitmap, IRemapModule dstRemapModule)
        {
            IRemapModule srcRemapModule;
            switch (bitmap.PixelOrder)
            {
                case PixelOrder.ARGB:
                    srcRemapModule = ARGBRemapModule.Instance;
                    break;
                case PixelOrder.GrayScale:
                    srcRemapModule = GrayScaleRemapModule.Instance;
                    break;
                case PixelOrder.GrayScaleAlpha:
                    srcRemapModule = GrayScaleARemapModule.Instance;
                    break;
                case PixelOrder.RGB:
                    srcRemapModule = RGBRemapModule.Instance;
                    break;
                case PixelOrder.RGBA:
                    srcRemapModule = RGBARemapModule.Instance;
                    break;
                default:
                    throw new ArgumentException("EasyBitmap pixel order is not supported.", nameof(bitmap));
            }
            return RemapColors(bitmap.ImageData, srcRemapModule, dstRemapModule);
        }
    }
}
