using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Easy.Remap
{
    /// <summary>
    /// Remap module interface.
    /// </summary>
    public interface IRemapModule
    {
        /// <summary>
        /// Get size of pixel in bytes.
        /// </summary>
        int ColorSize { get; }

        void Remap(IRemapModule remapModule, Span<byte> data, int positionX, int positionY, int width, int height, Stream dst);

        /// <summary>
        /// Get color of pixel of coordinate (positionX, positionY) from a Span of pixel bytes.
        /// </summary>
        /// <param name="span">Pixel bytes</param>
        /// <param name="positionX">Coordinate X.</param>
        /// <param name="positionY">Coordinate Y.</param>
        /// <param name="width">Width of image.</param>
        /// <param name="height">Height of image.</param>
        /// <returns></returns>
        Color GetColor(Span<byte> span, int positionX, int positionY, int width, int height);
    }
}
