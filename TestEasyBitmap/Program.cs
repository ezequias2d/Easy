using Easy;
using Easy.GDI;
using Easy.Remap;
using System;
using System.Drawing;
using System.IO;

namespace TestEasyBitmap
{
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap bitmap = new Bitmap("Resources/wall.png");
            EasyBitmap easyBitmap = bitmap.ToEasyBitmap(PixelOrder.RGB);
            easyBitmap.Flip(FlipMode.Horizontal);
            using (var file = File.OpenWrite("Resources/fliped.esbm"))
            {
                easyBitmap.ImageData = Remap.RemapColors(Remap.RemapColors(easyBitmap, GrayScaleRemapModule.Instance), GrayScaleRemapModule.Instance, RGBRemapModule.Instance);
                easyBitmap.Save(file, 0, 0);
            }
        }
    }
}
