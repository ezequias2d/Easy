using Easy.Huffman;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Easy
{
    public class EasyHuffman
    {

        /// <summary>
        /// Encode data with EasyLZ compression with header.
        /// </summary>
        /// <param name="src">Source data.</param>
        /// <param name="lengthBits">Bits used for word copy size (maximum is 7), (8 - lenghtBits) is the bits used to locate the copy.</param>
        /// <param name="dst">Destination data.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Encode(Span<byte> src, Span<byte> dst)
        {
            int dstPosition = 0;
            uint uncompressedSize = (uint)src.Length;

            //write fish header
            Span<byte> ESHF = new Span<byte>(Encoding.ASCII.GetBytes("ESHF"));
            ESHF.CopyTo(dst);

            dstPosition += ESHF.Length;

            //write uncrompress size
            dst[dstPosition++] = (byte)(uncompressedSize >> 24);
            dst[dstPosition++] = (byte)(uncompressedSize >> 16);
            dst[dstPosition++] = (byte)(uncompressedSize >> 8);
            dst[dstPosition++] = (byte)(uncompressedSize);

            return ESHF.Length + 4 + RawEncode(src, dst.Slice(dstPosition));
        }

        /// <summary>
        /// Compress data with EasyLZ without header.
        /// </summary>
        /// <param name="src">Source data.</param>
        /// <param name="dst">Destination data.</param>
        /// <returns>Compressed data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RawEncode(Span<byte> src, Span<byte> dst)
        {
            Tree<byte> tree = new Tree<byte>(src);
            uint srcPosition = 0;
            BitStream stream = new BitStream(null, dst);

            Serialize(tree.Root, stream);

            while (srcPosition < src.Length)
            {
                (byte count, ulong code) pair = tree[src[(int)(srcPosition++)]];

                while(pair.count > 0)
                {
                    stream.Write((byte)(pair.code & 1));
                    pair.code >>= 1;
                    pair.count--;
                }
            }
            stream.Dispose();

            return stream.WritePosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Serialize(Tree<byte>.Node node, BitStream stream)
        {
            if (node.IsLeaf)
            {
                stream.Write(1);
                stream.WriteByte(node.Pair.Key);
            }
            else
            {
                stream.Write(0);
                if(node.Left != null)
                    Serialize(node.Left, stream);
                if (node.Right != null)
                    Serialize(node.Right, stream);
            }
        }

        private static Tree<byte>.Node Deserialize(BitStream stream)
        {
            throw new NotImplementedException();
        }


    }
}
