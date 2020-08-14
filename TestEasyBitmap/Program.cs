using Easy;
using Easy.GDI;
using Easy.Remap;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using TestEasyBitmap.Properties;

namespace TestEasyBitmap
{
    class Program
    {
        //static void Main(string[] args)
        //{
        //    byte[] data = File.ReadAllBytes("Resources/enwik8");
        //    byte[] compressed = new byte[Easy.EasyLZ.MaxLengthEncode(data.Length)];
        //    int count = Easy.EasyLZ.Encode(data, compressed, 0);

        //    File.WriteAllBytes("Resources/enwik8.eslz", new Span<byte>(compressed, 0, count).ToArray());
        //}

        static void Main(string[] args)
        {
            byte[] data = File.ReadAllBytes("Resources/enwik8");
            byte[] compressed = new byte[data.Length];

            int count = EasyHuffman.Encode(data, compressed);

            Console.Read();
        }
    }
}
