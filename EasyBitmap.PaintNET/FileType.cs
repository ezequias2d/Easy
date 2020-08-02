using PaintDotNet;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using Easy.GDI;

namespace Easy.Paint.NET.Plugin
{
    public sealed class EasyBitmapFactory : IFileTypeFactory
    {
        public FileType[] GetFileTypeInstances()
        {
            return new[] { new EasyBitmapPlugin() };
        }
    }

    [PluginSupportInfo(typeof(PluginSupportInfo))]
    internal class EasyBitmapPlugin : FileType<EasyBitmapSaveConfigToken, EasyBitmapSaveConfigWidget>
    {

        /// <summary>
        /// Constructs a ExamplePropertyBasedFileType instance
        /// </summary>
        internal EasyBitmapPlugin()
            : base(
                "EasyBitmap (*.esbm)",
                new FileTypeOptions
                {
                    LoadExtensions = new string[] { ".esbm" },
                    SaveExtensions = new string[] { ".esbm" },
                    SupportsCancellation = false,
                    SupportsLayers = false
                })
        {
        }

        protected override EasyBitmapSaveConfigToken OnCreateDefaultSaveConfigTokenT()
        {
            return new EasyBitmapSaveConfigToken();
        }

        protected override EasyBitmapSaveConfigWidget OnCreateSaveConfigWidgetT()
        {
            return new EasyBitmapSaveConfigWidget();
        }

        public static Stream Compress(Stream input)
        {
            MemoryStream stream = new MemoryStream();

            using (DeflateStream compressionStream = new DeflateStream(stream, System.IO.Compression.CompressionMode.Compress, true))
            {
                input.CopyTo(compressionStream);
            }
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static Stream Decompress(Stream input)
        {
            MemoryStream stream = new MemoryStream();

            using (DeflateStream decompressionStream = new DeflateStream(input, System.IO.Compression.CompressionMode.Decompress, true))
            {
                decompressionStream.CopyTo(stream);
            }
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// Saves a document to a stream respecting the properties
        /// </summary>

        protected override void OnSaveT(Document input, Stream output, EasyBitmapSaveConfigToken token, Surface scratchSurface, ProgressEventHandler progressCallback)
        {

            using (RenderArgs args = new RenderArgs(scratchSurface))
            {
                // Render a flattened view of the Document to the scratch surface.
                input.Render(args, true);
            }

            Bitmap bmp = scratchSurface.CreateAliasedBitmap();

            //IZIBitmap izi = new IZIBitmap(bmp, token.PixelOrder);
            EasyBitmap izi = bmp.ToEasyBitmap(token.PixelOrder);
            izi.Save(output, (CompressionMode)token.Compression, (FilterMode)token.Filter);
            
            bmp.Dispose();
        }

        /// <summary>
        /// Creates a document from a stream
        /// </summary>
        protected override Document OnLoad(Stream input)
        {
            Document doc = null;

            // Create a background layer.
            BitmapLayer layer;

            EasyBitmap easyBitmap = new EasyBitmap(input);
            input.Close();
            Bitmap bmp = easyBitmap.ToBitmap();

            // Create a new Document.
            doc = new Document(bmp.Width, bmp.Height);

            Surface surface = Surface.CopyFromBitmap(bmp, false);
            layer = new BitmapLayer(surface);
            layer.Name = "Layer N";
            layer.Visible = true;
            layer.Opacity = 255;
            bmp.Dispose();

            doc.Layers.Add(layer);

            return doc;
        }
    }
}
