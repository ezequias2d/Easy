using Easy.Remap;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Easy.GDI
{
    public static class EasyBitmapGDIExtensions
    {
        private static unsafe Bitmap BitmapFromValues(int width, int height, byte[] values, PixelOrder pixelOrder)
        {
            Bitmap bitmap;
            PixelFormat pixelFormat;
            int pixelSizeDst = 0;
            int pixelSizeSrc = EasyBitmap.CalculatePixelBytes(pixelOrder);
            switch (pixelOrder)
            {
                case PixelOrder.ARGB:
                case PixelOrder.RGBA:
                case PixelOrder.GrayScaleAlpha:
                    pixelFormat = PixelFormat.Format32bppArgb;
                    pixelSizeDst = 4;
                    break;
                case PixelOrder.GrayScale:
                    pixelFormat = PixelFormat.Format8bppIndexed;
                    pixelSizeDst = 1;
                    break;
                case PixelOrder.RGB:
                    pixelFormat = PixelFormat.Format24bppRgb;
                    pixelSizeDst = 3;
                    break;
                default:
                    throw new FormatException("PixelOrder is not supported.");
            }

            bitmap = new Bitmap(width, height, pixelFormat);

            if (pixelOrder == PixelOrder.GrayScale)
            {
                ColorPalette pallet = bitmap.Palette;
                for (int i = 0; i < 256; i++)
                {
                    pallet.Entries[i] = Color.FromArgb(255, i, i, i);
                }
                bitmap.Palette = pallet;
            }

            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height),
                                            ImageLockMode.WriteOnly,
                                            pixelFormat);
            IntPtr ptr = bmpData.Scan0;

            int bytes = bmpData.Stride * height;
            var rgbValues = new byte[bytes];

            int pixels = width * height;

            values = Remap.Remap.RemapColors(values, width, height, Remap.Remap.GetRemapModule(pixelOrder), ARGBRemapModule.Instance);

            fixed (byte* dst = rgbValues, src = values)
            {
                int pD;
                int pS;
                for (int i = 0; i < pixels; i++)
                {
                    pD = i * pixelSizeDst;
                    pS = i * pixelSizeSrc;
                    switch (pixelOrder)
                    {
                        case PixelOrder.ARGB:
                        case PixelOrder.RGBA:
                            dst[pD + 3] = src[pS];
                            dst[pD + 2] = src[pS + 1];
                            dst[pD + 1] = src[pS + 2];
                            dst[pD] = src[pS + 3];
                            break;
                        case PixelOrder.GrayScale:
                            dst[pD] = src[pS + 1];
                            break;
                        case PixelOrder.RGB:
                            dst[pD + 2] = src[pS + 1];
                            dst[pD + 1] = src[pS + 2];
                            dst[pD] = src[pS + 3];
                            break;
                        case PixelOrder.GrayScaleAlpha:
                            dst[pD + 3] = src[pS];
                            dst[pD + 2] = src[pS + 1];
                            dst[pD + 1] = src[pS + 1];
                            dst[pD] = src[pS + 1];
                            break;
                    }
                }
            }

            Marshal.Copy(rgbValues, 0, ptr, bytes);
            bitmap.UnlockBits(bmpData);
            return bitmap;
        }

        private static unsafe void SetColors(in byte a, in byte r, in byte g, in byte b, in byte* array, in int index)
        {
            array[index] = a;
            array[index + 1] = r;
            array[index + 2] = g;
            array[index + 3] = b;
        }

        private static unsafe byte[] ImageDataFromBitmap(Bitmap bitmap, PixelOrder pixelOrder)
        {
            int pixelSizeDst = EasyBitmap.CalculatePixelBytes(PixelOrder.ARGB);
            int pixelSizeSrc = 0;
            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    pixelSizeSrc = 4;
                    break;
                case PixelFormat.Format16bppGrayScale:
                    pixelSizeSrc = 2;
                    break;
                case PixelFormat.Format24bppRgb:
                    pixelSizeSrc = 3;
                    break;
                case PixelFormat.Format32bppRgb:
                    pixelSizeSrc = 4;
                    break;
                case PixelFormat.Format8bppIndexed:
                    pixelSizeSrc = 1;
                    break;
                default:
                    throw new FormatException("PixelFormat is not supported.");
            }

            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                            ImageLockMode.ReadOnly,
                                            bitmap.PixelFormat);
            IntPtr ptr = bmpData.Scan0;

            int bytes = bitmap.Width * bitmap.Height * pixelSizeDst;
            byte[] imageData = new byte[bytes];

            int pixels = bitmap.Width * bitmap.Height;


            byte* src = (byte*)ptr.ToPointer();
            fixed (byte* dst = imageData)
            {
                int p;
                byte aux;
                Color[] pallet = null;
                if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    pallet = bitmap.Palette.Entries;
                }

                for (int i = 0; i < pixels; i++)
                {
                    p = i * pixelSizeSrc;
                    switch (bitmap.PixelFormat)
                    {
                        case PixelFormat.Format32bppArgb:
                            SetColors(src[p + 3], src[p + 2], src[p + 1], src[p + 0], dst, i * pixelSizeDst);
                            break;
                        case PixelFormat.Format16bppGrayScale:
                            aux = (byte)(((src[p] * 256) + src[p + 1]) / 257);
                            SetColors(255, aux, aux, aux, dst, i * pixelSizeDst);
                            break;
                        case PixelFormat.Format24bppRgb:
                        case PixelFormat.Format32bppRgb:
                            SetColors(255, src[p + 2], src[p + 1], src[p + 0], dst, i * pixelSizeDst);
                            break;
                        case PixelFormat.Format8bppIndexed:
                            aux = src[p];
                            SetColors(255, pallet[aux].R, pallet[aux].G, pallet[aux].B, dst, i * pixelSizeDst);
                            break;

                    }
                }
            }
            bitmap.UnlockBits(bmpData);
            imageData = Remap.Remap.RemapColors(imageData, bitmap.Width, bitmap.Height, ARGBRemapModule.Instance, Remap.Remap.GetRemapModule(pixelOrder));
            return imageData;
        }
        public static EasyBitmap ToEasyBitmap(this Bitmap bitmap, PixelOrder pixelOrder)
        {
            int w = bitmap.Width;
            int h = bitmap.Height;
            byte[] imageData = ImageDataFromBitmap(bitmap, pixelOrder);

            return new EasyBitmap(w, h, pixelOrder, imageData, 0, imageData.Length);
        }

        public static Bitmap ToBitmap(this EasyBitmap bitmap)
        {
            return BitmapFromValues(bitmap.Width, bitmap.Height, bitmap.ImageData, bitmap.PixelOrder);
        }
    }
}
