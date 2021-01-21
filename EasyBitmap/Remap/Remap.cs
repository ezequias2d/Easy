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

        public static IRemapModule GetRemapModule(PixelFormat pixelOrder)
        {
            switch (pixelOrder)
            {
                case PixelFormat.ARGB:
                    return ARGBRemapModule.Instance;
                case PixelFormat.GrayScale:
                    return GrayScaleRemapModule.Instance;
                case PixelFormat.GrayScaleAlpha:
                    return GrayScaleARemapModule.Instance;
                case PixelFormat.RGB:
                    return RGBRemapModule.Instance;
                case PixelFormat.RGBA:
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
