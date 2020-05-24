using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Easy
{
    public class EasyArchiveEntry : IDisposable
    {
        internal MemoryStream _uncompressedData;

        public EasyArchive Archive { get; internal set; }
        public CompressionType Compression { get; set; }
        public ulong CompressedLength { get; internal set; }
        public string FullName { get; internal set; }
        public long LastWriteTime { get; set; }
        public ulong Length { get; internal set; }
        public string Name => Path.GetFileName(FullName);
        internal ulong Position { get; set; }
        internal byte PearsonHashingUncompressed { get; set; }
        internal byte PearsonHashingCompressed { get; set; }

        internal MemoryStream UncompressedData
        {
            get
            {
                if(_uncompressedData == null)
                {
                    if(Length == 0)
                    {
                        _uncompressedData = new MemoryStream();
                    }
                    else
                    {
                        //load from EasyArchive
                        byte[] data = Archive.ReadEntry(this);

                        switch (Compression)
                        {
                            case CompressionType.EasyLZ:
                                data = GetDecompressDataEasyLZ(data);
                                break;
                            case CompressionType.LZ4:
                                data = GetDecompressDataLZ4(data);
                                break;
                            case CompressionType.Deflate:
                                data = GetDecompressDataDeflate(data);
                                break;
                        }

                        _uncompressedData = new MemoryStream(data);


                        Length = (ulong)_uncompressedData.Length;
                    }
                }
                return _uncompressedData;
            }
        }

        internal EasyArchiveEntry(CompressionType compression = CompressionType.EasyLZ)
        {
            Compression = compression;
        }

        public void Delete()
        {
            Archive.DeleteEntry(this);
        }

        public Stream Open()
        {
            UncompressedData.Seek(0, SeekOrigin.Begin);
            return new StreamWrapper(UncompressedData, Archive.Mode);
        }

        private Span<byte> GetCompressionDataEasyLZ(byte[] uncompressed)
        {
            byte[] compressed = new byte[(int)(Length * 1.125f + 16)];
            int size = EasyLZ.Encode(uncompressed, 2, compressed);
            return new Span<byte>(compressed, 0, size);
        }

        private Span<byte> GetCompressionDataLZ4(byte[] uncompressed)
        {
            byte[] compressed = new byte[LZ4Codec.MaximumOutputSize(uncompressed.Length)];
            int size = LZ4Codec.Encode(
                uncompressed, 0, uncompressed.Length,
                compressed, 0, compressed.Length);
            return new Span<byte>(compressed, 0, size);
        }

        private Span<byte> GetCompressionDataDeflate(byte[] uncompressed)
        {
            using(MemoryStream uncompressedStream = new MemoryStream(uncompressed))
            {
                using (MemoryStream compressed = new MemoryStream())
                {
                    using (DeflateStream compressionStream = new DeflateStream(compressed, CompressionMode.Compress, true))
                    {
                        uncompressedStream.CopyTo(compressionStream);
                    }
                    compressed.Seek(0, SeekOrigin.Begin);

                    return new Span<byte>(compressed.ToArray());
                }
            }
        }

        private byte[] GetDecompressDataEasyLZ(byte[] compressed)
        {
            return EasyLZ.Decode(compressed);
        }

        private byte[] GetDecompressDataLZ4(byte[] compressed)
        {
            byte[] target = new byte[Length];
            LZ4Codec.Decode(compressed, 0, compressed.Length, target, 0, target.Length);
            return target;
        }

        private byte[] GetDecompressDataDeflate(byte[] compressed)
        {
            using (MemoryStream compressedStream = new MemoryStream(compressed))
            {
                using(MemoryStream decompressedStream = new MemoryStream((int)Length))
                {
                    using (DeflateStream decompressionStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedStream);
                    }
                    decompressedStream.Seek(0, SeekOrigin.Begin);
                    return decompressedStream.ToArray();
                }
            }
        }

        internal byte[] GetCompressedData()
        {
            byte[] uncompressed = UncompressedData.ToArray();
            Span<byte> compressed;

            switch (Compression)
            {
                case CompressionType.EasyLZ:
                    compressed = GetCompressionDataEasyLZ(uncompressed);
                    break;
                case CompressionType.LZ4:
                    compressed = GetCompressionDataLZ4(uncompressed);
                    break;
                case CompressionType.Deflate:
                    compressed = GetCompressionDataDeflate(uncompressed);
                    break;
                default:
                    compressed = uncompressed;
                    break;
            }

            CompressedLength = (ulong)compressed.Length;
            PearsonHashingCompressed = PearsonHashing.Hash(compressed);
            PearsonHashingUncompressed = PearsonHashing.Hash(uncompressed);
            return compressed.ToArray();
        }

        public override string ToString()
        {
            return FullName;
        }

        public void Dispose()
        {
            if(_uncompressedData != null)
            {
                _uncompressedData.Close();
                _uncompressedData.Dispose();
            }
        }
    }
}
