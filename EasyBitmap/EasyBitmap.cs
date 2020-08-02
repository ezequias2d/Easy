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
          |                         0 = ARGB
          |                         1 = RGBA
          |                         2 = GrayScale
                                    3 = RGB
          |                         4 = GrayScale-A
      23  |        4            | Compression:
          |                         0 - No compression
          |                         1 - Deflate
          |                         2 - LZ4
          |                         3 - RLE
          |                         4 - EasyLZ
      27  |        4            | Filter:
          |                         0 - No filter
          |                         1 - Axis
          |                         2 - Sub
      31  |        8            | Size of image data in bytes without compression.
            ----------- Image data -----------
      39        IMAGE DATA 
         */

    public enum PixelOrder : byte
    {
        ARGB = 0,
        RGBA = 1,
        GrayScale = 2,
        RGB = 3,
        GrayScaleAlpha = 4
    }

    public enum FlipMode
    {
        None,
        Horizontal,
        Vertical
    }

    public enum CompressionMode : uint
    {
        None = 0,
        Deflate = 1,
        LZ4 = 2,
        RLE = 3,
        EasyLZ = 4
    }

    public enum FilterMode : uint
    {
        None = 0,
        Axis = 1,
        Sub = 2
    }

    public class EasyBitmap
    {
        private const string HeaderSignature = "ESBM";

        private static IReadOnlyList<ICompression> Compressions;
        private static IReadOnlyList<IFilter> Filters;

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

        /// <summary>
        /// Width of image.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of image.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// PixelOrder format.
        /// </summary>
        public PixelOrder PixelOrder { get; private set; }

        /// <summary>
        /// Pixel image data.
        /// </summary>
        public byte[] ImageData { get; private set; }

        /// <summary>
        /// Get bytes per pixel in this image.
        /// </summary>
        public int PixelBytes
        {
            get
            {
                return CalculatePixelBytes(PixelOrder);
            }
        }

        /// <summary>
        /// Create EasyBitmap from variables.
        /// </summary>
        /// <param name="width">Width of image.</param>
        /// <param name="height">Height of image.</param>
        /// <param name="pixelOrder">PixelOrder of image.</param>
        /// <param name="imageData">Pixel image data.</param>
        public EasyBitmap(int width, int height, PixelOrder pixelOrder, Span<byte> imageData)
        {
            Width = width;
            Height = height;
            PixelOrder = pixelOrder;
            ImageData = new byte[width * height * PixelBytes];
            imageData.CopyTo(ImageData);

            if (ImageData == null || ImageData.Length == 0)
            {
                throw new Exception("Image data is very low size.");
            }
        }

        /// <summary>
        /// Create a new EasyBitmap object from stream that contain a EasyBitmap image file.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        /// <param name="leaveOpen">Leave stream open</param>
        public EasyBitmap(Stream stream, bool leaveOpen = false)
        {
            Open(stream, leaveOpen);
        }

        /// <summary>
        /// Load image from stream for this EasyBitmap.
        /// </summary>
        /// <param name="stream">Input stream.</param>
        /// <param name="leaveOpen">Leave stream open.</param>
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
                byte[] headerSignature = reader.ReadBytes(HeaderSignature.Length);

                if (Encoding.ASCII.GetString(headerSignature) != HeaderSignature)
                {
                    throw new FormatException("Invalid file signature.");
                }
                stream.Seek(14, SeekOrigin.Begin);

                width = (int)reader.ReadUInt32();
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

        /// <summary>
        /// Calculate bytes for a PixelOrder format.
        /// </summary>
        /// <param name="pixelOrder">PixelOrder format.</param>
        /// <returns>Number of bytes used per pixel in the PixelOrder format.</returns>
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

        /// <summary>
        /// Record image to stream.
        /// </summary>
        /// <param name="stream">Output stream.</param>
        /// <param name="compress">Compression mode.</param>
        /// <param name="filter">Filter mode.</param>
        /// <param name="reporter"></param>
        public void Save(Stream stream, CompressionMode compress, FilterMode filter, StatusReporter reporter = null)
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

        /// <summary>
        /// Swap two pixels of image.
        /// </summary>
        /// <param name="x1">X1 Euclidean coordinate.</param>
        /// <param name="y1">Y1 Euclidean coordinate.</param>
        /// <param name="x2">X2 Euclidean coordinate.</param>
        /// <param name="y2">Y2 Euclidean coordinate.</param>
        public void Swap(int x1, int y1, int x2, int y2)
        {
            int bytes = PixelBytes;

            int p1 = (x1 + y1 * Width) * bytes;
            int p2 = (x2 + y2 * Width) * bytes;

            byte aux;

            for (int i = 0; i < bytes; i++)
            {
                aux = ImageData[p1 + i];
                ImageData[p1 + i] = ImageData[p2 + i];
                ImageData[p2 + i] = aux;
            }
        }

        /// <summary>
        /// Flip bitmap in FlipMode mode.
        /// </summary>
        /// <param name="flipMode">Flip mode.</param>
        public void Flip(FlipMode flipMode)
        {
            if(flipMode == FlipMode.Vertical)
            {
                int w = Width - 1;
                for (int x = 0; x < Width / 2; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        Swap(x, y, w, y);
                    }
                    w--;
                }
            }
            else if(flipMode == FlipMode.Horizontal)
            {
                for (int x = 0; x < Width; x++)
                {
                    var h = Height - 1;
                    for (int y = 0; y < Height / 2; y++)
                    {
                        Swap(x, y, x, h);
                        h--;
                    }
                }
            }
        }
    }
}
