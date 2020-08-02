using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Easy.Remap
{
    public static class Remap
    {
        public static byte[] RemapColors(Span<byte> src, int width, int height, IRemapModule srcRemapModule, IRemapModule dstRemapModule)
        {
            Span<byte> srcSpan = src;

            using (MemoryStream dst = new MemoryStream())
            {
                int positionX = 0;
                int positionY = 0;
                while(positionX < width && positionY < height)
                {
                    dstRemapModule.Remap(srcRemapModule, srcSpan, positionX, positionY, width, height, dst);
                    
                    positionX++;
                    
                    if(positionX >= width)
                    {
                        positionX = 0;
                        positionY++;
                    }
                }

                return dst.ToArray();
            }
        }

        public static IRemapModule GetRemapModule(PixelOrder pixelOrder)
        {
            switch (pixelOrder)
            {
                case PixelOrder.ARGB:
                    return ARGBRemapModule.Instance;
                case PixelOrder.GrayScale:
                    return GrayScaleRemapModule.Instance;
                case PixelOrder.GrayScaleAlpha:
                    return GrayScaleARemapModule.Instance;
                case PixelOrder.RGB:
                    return RGBRemapModule.Instance;
                case PixelOrder.RGBA:
                    return RGBARemapModule.Instance;
                default:
                    throw new ArgumentException("EasyBitmap pixel order is not supported.");
            }
        }

        public static byte[] RemapColors(EasyBitmap bitmap, IRemapModule dstRemapModule)
        {
            IRemapModule srcRemapModule = GetRemapModule(bitmap.PixelOrder);
            return RemapColors(bitmap.ImageData, bitmap.Width, bitmap.Height, srcRemapModule, dstRemapModule);
        }
    }
}
