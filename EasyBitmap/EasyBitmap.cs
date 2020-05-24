using Easy.Compression;
using Easy.Filter;
using System;
using System.Collections.Generic;
using System.IO;

using System.Text;
using System.Threading.Tasks;

namespace Easy
{
    /*
     File:
       P  |         Values      |       Purpose                             
       0  | 0x45 0x53 0x42 0x4D | ESBM in ASCII, identify the format easily.
       3  | 0x0D 0x0A           | Newline
       5  | 0x03                | End-of-text charecter(ETX)
          |
          | ----------- Info header -----------
          |      Bytes
       6  |        8            | File size
      14  |        4            | Width
      18  |        4            | Height
      22  |        1            | Pixel order:
          |                         0 = RGBA
          |                         1 = ARGB(Default)
          |                         2 = GrayScale
          |                         3 = GrayScale-A
      23  |        4            | Compression:
          |                         0 - No compression
          |                         1 - Deflate
          |                         2 - LZ4
      27  |        4            | Filter:
          |                         0 - No filter
          |                         1 - Axis
          |                         2 - Left
      31  |        8            | Size of image data in bytes without compression.

            ----------- Image data -----------
      39        IMAGE DATA 
         */

    public enum PixelOrder
    {
        ARGB = 0,
        RGBA = 1,
        GrayScale = 2,
        RGB = 3,
        GrayScaleAlpha = 4
    }
    public class EasyBitmap
    {
        private const string HeaderSignature = "ESBM";

        public static IReadOnlyList<ICompression> Compressions;
        public static IReadOnlyList<IFilter> Filters;

        static EasyBitmap()
        {
            List<ICompression> compressions = new List<ICompression>();
            compressions.Add(new DeflateCompression());
            compressions.Add(new LZ4Compression());
            compressions.Add(new RLECompression());
            compressions.Add(new EasyLZCompression());
            Compressions = compressions;

            List<IFilter> filters = new List<IFilter>();
            filters.Add(new AxisFilter());
            filters.Add(new SubFilter());

            Filters = filters;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public PixelOrder PixelOrder { get; private set; }

        public byte[] ImageData;

        public int PixelBytes
        {
            get
            {
                return CalculatePixelBytes(PixelOrder);
            }
        }

        public EasyBitmap(int width, int height, PixelOrder pixelOrder, byte[] imageData, int index, int length)
        {
            Width = width;
            Height = height;
            PixelOrder = pixelOrder;
            ImageData = new byte[width * height * PixelBytes];
            Array.Copy(imageData, index, ImageData, 0, length);

            if (ImageData == null || ImageData.Length == 0)
            {
                throw new Exception("Image data is very low size.");
            }
        }

        public EasyBitmap(Stream stream, bool leaveOpen = false)
        {
            Open(stream, leaveOpen);
        }

        public void Open(Stream stream, bool leaveOpen = false)
        {
            int width;
            int height;
            PixelOrder pixelOrder;
            uint compression;
            uint filter;
            ulong imageSize;
            // The stream paint.net hands us must not be closed.
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
            {
                // Read and validate the file header.
                byte[] headerSignature = reader.ReadBytes(3);

                if (Encoding.ASCII.GetString(headerSignature) != HeaderSignature)
                {
                    throw new FormatException("Invalid file signature.");
                }
                stream.Seek(14, SeekOrigin.Begin);

                width = (int)reader.ReadInt32();
                height = (int)reader.ReadUInt32();
                pixelOrder = (PixelOrder)reader.ReadByte();
                compression = reader.ReadUInt32();
                filter = reader.ReadUInt32();
                imageSize = reader.ReadUInt64();
            }

            Width = width;
            Height = height;
            PixelOrder = pixelOrder;

            MemoryStream image = new MemoryStream();
            if (compression == 0)
            {
                Task task = stream.CopyToAsync(image, (int)imageSize);
                task.Wait();
            }
            else
            {
                ICompression comp = Compressions[(int)compression - 1];
                comp.Decompress(stream, image);
            }
            byte[] imageData = image.ToArray();

            if (filter == 0)
            {
                ImageData = imageData;
            }
            else
            {
                IFilter filt = Filters[(int)filter - 1];
                ImageData = filt.Defilter((ulong)(Width * PixelBytes), (ulong)Height, imageData);
            }

            image.Close();
            image.Dispose();

            if (!leaveOpen)
            {
                stream.Flush();
                stream.Close();
            }
        }

        public static int CalculatePixelBytes(PixelOrder pixelOrder)
        {
            switch (pixelOrder)
            {
                case PixelOrder.ARGB:
                case PixelOrder.RGBA:
                    return 4;
                case PixelOrder.GrayScale:
                    return 1;
                case PixelOrder.RGB:
                    return 3;
                case PixelOrder.GrayScaleAlpha:
                    return 2;
            }
            return 0;
        }

        public void Save(Stream stream, uint compress, uint filter, StatusReporter reporter = null)
        {
            // The stream paint.net hands us must not be closed.
            using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                // Write the file header.
                writer.Write(Encoding.ASCII.GetBytes(HeaderSignature));
                writer.Write((byte)0x0D);
                writer.Write((byte)0x0A);
                writer.Write((byte)0x03);


                byte[] imageData;
                if (filter == 0)
                {
                    imageData = ImageData;
                    reporter?.UpdateStatus(StatusReporter.Status.Filter, 1);
                }
                else
                {
                    IFilter filt = Filters[(int)filter - 1];
                    filt.StatusReporter = reporter;
                    imageData = filt.Filter((ulong)(Width * PixelBytes), (ulong)Height, ImageData);
                    filt.StatusReporter = null;
                }

                stream.Seek(39, SeekOrigin.Begin);
                if (compress == 0)
                {
                    stream.Write(imageData, 0, imageData.Length);
                }
                else
                {
                    using (MemoryStream memoryStream = new MemoryStream(imageData))
                    {
                        ICompression comp = Compressions[(int)compress - 1];
                        Stream useStrem = memoryStream;

                        if (reporter != null)
                        {
                            ObservableStream streamWrapper = new ObservableStream(memoryStream);
                            streamWrapper.Subscribe(reporter);
                            reporter.status = StatusReporter.Status.Compress;
                            useStrem = streamWrapper;
                        }
                        comp.Compress(useStrem, stream);
                        memoryStream.Close();
                    }
                }
                stream.Seek(0, SeekOrigin.End);


                ulong size = (ulong)stream.Position;

                stream.Seek(6, SeekOrigin.Begin);
                writer.Write(size);
                writer.Write((uint)Width);
                writer.Write((uint)Height);
                writer.Write((byte)PixelOrder);
                writer.Write((uint)(compress));
                writer.Write((uint)(filter));
                writer.Write((ulong)imageData.Length);

                writer.Flush();
                writer.Close();
            }
        }
    }
}
