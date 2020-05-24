using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Easy
{
    public static class EasyLZ
    {

        /// <summary>
        /// Result of Search function
        /// </summary>
        private ref struct SearchResult
        {
            public int length;
            public int offset;
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SearchResult SearchInternal(Span<byte> src, HashTable hashTable, in int position, in int maxLength, in int maxOffset)
        {
            hashTable.Update();
            SearchResult result = new SearchResult { length = 1, offset = 0 };
            int length;
            if (position + 2 < src.Length)
            {
                int searchIdx = hashTable.Get(src, position);
                if (searchIdx != -1)
                {
                    while ((position - searchIdx) < maxOffset && searchIdx >= 0 && src[position + result.length] == src[searchIdx + result.length])
                    {
                        length = 0;
                        while (position + length < src.Length && length < maxLength && src[searchIdx + length] == src[position + length])
                        {
                            length++;
                        }
                        if (length > result.length)
                        {
                            result.offset = position - searchIdx;
                            result.length = length;
                        }

                        if (length < maxLength)
                            searchIdx = hashTable.Get(src, position);
                        else
                            searchIdx = -1;
                    }
                }
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddHashTable(in Span<byte> src, in HashTable hashTable, in int position, in int resultLenth)
        {
            int idx = position;
            int end = position + resultLenth;
            while (idx < end && idx < src.Length - 1)
            {
                hashTable.Add(src, idx++);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SearchResult Search(Span<byte> src, HashTable hashTable, in int position, in int maxLength, in int maxOffset, ref bool previewFlag, ref SearchResult previewResult)
        {
            SearchResult result;

            if (previewFlag)
            {
                previewFlag = false;
                result = previewResult;
            }
            else
            {
                result = SearchInternal(src, hashTable, position, maxLength, maxOffset);
            }

            AddHashTable(src, hashTable, position, 1);

            if (result.length >= 2 && result.length < maxLength)
            {
                previewResult = SearchInternal(src, hashTable, position + 1, maxLength, maxOffset);
                if (previewResult.length > result.length + 1)
                {
                    previewFlag = true;
                    result.length = 1;
                }
            }

            AddHashTable(src, hashTable, position + 1, result.length - 1);
            return result;
        }

        public static int MaxLengthEncode(int length)
        {
            return (int)(length * 1.125f + 9);
        }

        public static int MaxLengthRawEncode(int length)
        {
            return (int)(length * 1.125f + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Encode(Span<byte> src, byte lengthBits, Span<byte> dst)
        {
            int dstPosition = 0;
            uint uncompressedSize = (uint)src.Length;

            //write fish header
            Span<byte> ESLZ = new Span<byte>(Encoding.ASCII.GetBytes("ESLZ"));
            ESLZ.CopyTo(dst);

            dstPosition += ESLZ.Length;

            //write uncrompress size
            dst[dstPosition++] = (byte)(uncompressedSize >> 24);
            dst[dstPosition++] = (byte)(uncompressedSize >> 16);
            dst[dstPosition++] = (byte)(uncompressedSize >> 8);
            dst[dstPosition++] = (byte)(uncompressedSize);

            return RawEncode(src, lengthBits, dst, dstPosition);
        }

        /// <summary>
        /// Compress data with EasyLZ without header.
        /// </summary>
        /// <param name="src">Source data</param>
        /// <param name="matchBits">Max 7</param>
        /// <returns>Compressed data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RawEncode(Span<byte> src, byte lengthBits, Span<byte> dst, int startDst = 0)
        {
            int position = 0;
            int length = src.Length;
            SearchResult match;

            int maxOffset = (int)Math.Pow(2, 8 - lengthBits);
            int maxLenght = (int)Math.Pow(2, lengthBits) + 1;
            HashTable hashTable = new HashTable(65536, maxOffset);

            int dataPosition = startDst;
            int bufferPosition = 1;
            byte code = 0;
            byte codeCount = 0;

            bool previewFlag = false;
            SearchResult previewResult = default(SearchResult);

            //header
            dst[dataPosition++] = lengthBits;

            while (position < length)
            {
                match = Search(src, hashTable, position, maxLenght, maxOffset, ref previewFlag, ref previewResult);

                if (match.length <= 1)
                {
                    //copia, por não economizar dados, ja que vai consumir o mesmo ou mais.
                    dst[dataPosition + (bufferPosition++)] = src[position++];

                    //seta flag de copia
                    code |= (byte)(0x80 >> codeCount);
                }
                else
                {
                    Debug.Assert(match.offset > 0, "Match have offset 0.");
                    // reduz 1 para mapear 0 para 1.
                    int distance = match.offset - 1;

                    // reduz 2 para mapear de 0 para 2
                    int size = match.length - 2;

                    // compressed match size | match distance
                    byte b2 = (byte)((size << (8 - lengthBits)) | distance);

                    //byte1
                    dst[dataPosition + (bufferPosition++)] = b2;

                    position += match.length;
                }

                codeCount++;
                //full buffer and full code.
                if (codeCount == 8)
                {
                    dst[dataPosition] = code;
                    dataPosition += bufferPosition;

                    //reset buffer, code and count
                    code = 0;
                    codeCount = 0;
                    bufferPosition = 1;
                }
            }

            if (codeCount > 0)
            {
                dst[dataPosition] = code;
                dataPosition += bufferPosition;
            }
            return dataPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Decode(Span<byte> src)
        {
            int srcCount = 0;
            byte[] EALZ = new byte[4];

            EALZ[0] = src[srcCount++];
            EALZ[1] = src[srcCount++];
            EALZ[2] = src[srcCount++];
            EALZ[3] = src[srcCount++];

            // EALZ in ascii
            if (!(EALZ[0] == 0x45 && EALZ[1] == 0x53 && EALZ[2] == 0x4C && EALZ[3] == 0x5A))
            {
                throw new Exception("Invalid EasyLZ stream.");
            }

            uint uncompressedSize = 0;
            uncompressedSize |= (uint)src[srcCount++] << 24;
            uncompressedSize |= (uint)src[srcCount++] << 16;
            uncompressedSize |= (uint)src[srcCount++] << 8;
            uncompressedSize |= (uint)src[srcCount++] << 0;

            byte[] dst = new byte[uncompressedSize];

            RawDecode(src, dst, srcCount);

            return dst;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void RawDecode(Span<byte> src, Span<byte> dst, int srcPosition)
        {
            fixed (byte* pSrc = src, pDst = dst)
            {
                byte* pDstPosition = pDst;
                byte* pDstLength = pDstPosition + dst.Length;
                byte* pSrcPosition = pSrc + srcPosition;

                byte code = 0;
                byte codeCount = 0;

                byte maxMatchBits = *pSrcPosition;
                pSrcPosition++;

                byte* pPosition;
                int length;

                byte aux;
                do
                {
                    if (codeCount == 0)
                    {
                        code = *pSrcPosition;
                        codeCount = 8;
                        pSrcPosition++;
                    }

                    // copy
                    if ((code & 0x80) != 0)
                    {
                        *pDstPosition = *pSrcPosition;
                        pDstPosition++;
                        pSrcPosition++;
                    }
                    else
                    {
                        aux = *pSrcPosition;
                        pSrcPosition++;

                        // mapei de 0 para 1
                        //uint dist = (uint)(byte1 & (0xFF >> maxMatchBits)) + 1;

                        // mapeia de 0 para 3
                        length = (aux >> (8 - maxMatchBits)) + 2;

                        // acha posição
                        pPosition = (pDstPosition - (aux & (0xFF >> maxMatchBits)) - 1);

                        MemoryCopy(pPosition, pDstPosition, length, (int)(pDstPosition - pPosition));
                        pDstPosition += length;
                    }

                    code <<= 1;
                    codeCount--;
                } while (pDstPosition < pDstLength);

            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void MemoryCopy(byte* source, byte* destination, int length, int overlapPosition)
        {
            while (length > 0)
            {
                length--;
                *destination = *source;
                destination++;
                source++;
            }
        }

    }
}
