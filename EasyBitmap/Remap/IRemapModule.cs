using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Easy.Remap
{
    public interface IRemapModule
    {
        int ColorSize { get; }

        void Remap(ref Color color, Stream dst);

        Color GetColor(Span<byte> span);
    }
}
