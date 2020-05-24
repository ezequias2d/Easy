using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Easy
{
    /*
        (numbers is in little endian)

        ESAR - identifies EasyArchive(ASCII)
        8 bytes - number of files

        for each file:
            2 bytes - compression
            4 bytes - name file size in bytes(N)
            N bytes - file name(UTF8)
            8 bytes - position in file
            8 bytes - last write time(unix time)
            8 bytes - uncompressed size
            8 bytes - compressed size
            1 byte  - PearsonHashing in compressed data
            1 byte  - PearsonHashing in uncompressed data

        *data file*
         */
    public class EasyArchive : IDisposable
    {
        private static readonly byte[] ESAR = Encoding.ASCII.GetBytes("ESAR");
        public enum EasyArchiveMode
        {
            Read,
            Create,
            Update
        }

        private Stream _stream;
        private IList<EasyArchiveEntry> _entries;
        private bool _leaveOpen;
        private long startPosition;

        public ReadOnlyCollection<EasyArchiveEntry> Entries { get; private set; }
        
        public EasyArchiveMode Mode { get; private set; }

        public EasyArchive(Stream stream, EasyArchiveMode mode, bool leaveOpen)
        {
            Mode = mode;
            _leaveOpen = leaveOpen;
            _entries = new List<EasyArchiveEntry>();
            Entries = new ReadOnlyCollection<EasyArchiveEntry>(_entries);
            _stream = stream;
            startPosition = stream.Position;
            if(mode != EasyArchiveMode.Create)
                LoadFromStream(stream);
        }

        private void CheckEasyArchive(Stream stream)
        {
            byte[] magic = new byte[ESAR.Length];
            stream.Read(magic, 0, ESAR.Length);
            for (int i = 0; i < ESAR.Length; i++)
            {
                if (magic[i] != ESAR[i])
                {
                    throw new Exception("The stream does not contain an EasyArchive");
                }
            }
        }

        private void LoadFromStream(Stream stream)
        {
            CheckEasyArchive(stream);

            using (BinaryStream reader = new BinaryStream(stream, true))
            {
                ulong entriesCount = reader.ReadUlong();
                for(ulong i = entriesCount; i > 0; i--)
                {
                    CompressionType compression = (CompressionType)reader.ReadUshort();
                    uint fileNameSize = reader.ReadUint();
                    byte[] fileName = reader.ReadBytes((int)fileNameSize);

                    EasyArchiveEntry entry = new EasyArchiveEntry();
                    entry.Compression = compression;
                    entry.Archive = this;
                    entry.CompressedLength = 0;
                    entry.FullName = Encoding.UTF8.GetString(fileName);
                    entry.Position = reader.ReadUlong();
                    entry.LastWriteTime = reader.ReadLong();
                    entry.Length = reader.ReadUlong();
                    entry.CompressedLength = reader.ReadUlong();
                    entry.PearsonHashingCompressed = reader.ReadByte();
                    entry.PearsonHashingUncompressed = reader.ReadByte();

                    _entries.Add(entry);
                }
            }
        }

        internal void DeleteEntry(EasyArchiveEntry entry)
        {
            _entries.Remove(entry);
        }

        internal byte[] ReadEntry(EasyArchiveEntry entry)
        {
            byte[] compressData = new byte[entry.CompressedLength];

            _stream.Seek((long)entry.Position + startPosition, SeekOrigin.Begin);
            _stream.Read(compressData, 0, (int)entry.CompressedLength);

            return compressData;
        }

        public EasyArchiveEntry CreateEntry(CompressionType compression = CompressionType.EasyLZ)
        {
            EasyArchiveEntry entry = new EasyArchiveEntry(compression);
            entry.Archive = this;
            _entries.Add(entry);
            return entry;
        }

        public EasyArchiveEntry CreateEntryFromFile(string file, string pathInArchive, CompressionType compression = CompressionType.EasyLZ)
        {
            EasyArchiveEntry entry = new EasyArchiveEntry(compression);
            entry.FullName = pathInArchive;
            entry.Archive = this;
            _entries.Add(entry);
            Stream stream = entry.Open();
            byte[] fileData = File.ReadAllBytes(file);
            stream.Write(fileData, 0, fileData.Length);
            entry.Length = (ulong)fileData.Length;
            return entry;
        }

        public EasyArchiveEntry[] CreateEntryFromPath(string path, CompressionType compression = CompressionType.EasyLZ)
        {
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            List<EasyArchiveEntry> entries = new List<EasyArchiveEntry>();
            for(int i = 0; i < files.Length; i++)
            {
                EasyArchiveEntry entry = CreateEntryFromFile(files[i], files[i].Substring(path.Length), compression);
                entries.Add(entry);
            }
            return entries.ToArray();
        }

        public void Dispose()
        {
            if(Mode != EasyArchiveMode.Read)
            {
                _stream.Position = startPosition;
                using (BinaryStream stream = new BinaryStream(_stream, true))
                {
                    stream.Write(ESAR);
                    stream.Write((ulong)_entries.Count);

                    List<(byte[] compressed, ulong compressedLength, long streamPosition)> list = new List<(byte[] uncompressed, ulong compressedLength, long streamPosition)>();
                    foreach (EasyArchiveEntry entry in _entries)
                    {
                        (byte[] compressed, ulong compressedLength, long streamPosition) element = (null, 0, 0);
                        element.compressed = entry.GetCompressedData();

                        byte[] fileName = Encoding.UTF8.GetBytes(entry.FullName);
                        uint fileNameSize = (uint)fileName.Length;

                        stream.Write((ushort)entry.Compression);
                        stream.Write(fileNameSize);
                        stream.Write(fileName);

                        element.streamPosition = stream.Position;
                        stream.Write(0ul);          //position

                        stream.Write(entry.LastWriteTime);
                        stream.Write(entry.Length);

                        stream.Write(entry.CompressedLength);          //compress length
                        element.compressedLength = entry.CompressedLength;

                        stream.Write(entry.PearsonHashingCompressed);      //PearsonHash compressed
                        stream.Write(entry.PearsonHashingUncompressed);

                        list.Add(element);
                    }

                    foreach ((byte[] compressed, ulong compressedLength, long streamPosition) entry in list)
                    {
                        long position = _stream.Position;
                        stream.Position = entry.streamPosition;
                        stream.Write((ulong)(position - startPosition));

                        stream.Position = position;
                        stream.Write(entry.compressed, 0, (int)entry.compressedLength);
                    }
                }
            }


            if (!_leaveOpen)
            {
                _stream.Close();
            }

            foreach(EasyArchiveEntry entry in Entries)
            {
                entry.Dispose();
            }
        }
    }
}
