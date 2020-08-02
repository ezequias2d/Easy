using System.IO;

namespace Easy.Compression
{
    /// <summary>
    /// A compression algorithm
    /// </summary>
    public interface ICompression
    {
        /// <summary>
        /// Compress an input stream to output stream.
        /// </summary>
        /// <param name="input">Input stream.</param>
        /// <param name="output">Output stream.</param>
        void Compress(Stream input, Stream output);

        /// <summary>
        /// Decompress an input stream to output stream.
        /// </summary>
        /// <param name="input">Input stream.</param>
        /// <param name="output">Output stream</param>
        void Decompress(Stream input, Stream output);
    }
}
