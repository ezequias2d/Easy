using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Easy
{
    public static class LittleEndianConverter
    {
        public static short ToShort(byte[] data, int startIndex = 0)
        {
            return BitConverter.ToInt16(CorrectOrder(data), startIndex);
        }

        public static ushort ToUshort(byte[] data, int startIndex = 0)
        {
            return BitConverter.ToUInt16(CorrectOrder(data), startIndex);
        }

        public static int ToInt(byte[] data, int startIndex = 0)
        {
            return BitConverter.ToInt32(CorrectOrder(data), startIndex);
        }

        public static uint ToUint(byte[] data, int startIndex = 0)
        {
            return BitConverter.ToUInt32(CorrectOrder(data), startIndex);
        }

        public static long ToLong(byte[] data, int startIndex = 0)
        {
            return BitConverter.ToInt64(CorrectOrder(data), startIndex);
        }

        public static ulong ToUlong(byte[] data, int startIndex = 0)
        {
            return BitConverter.ToUInt64(CorrectOrder(data), startIndex);
        }

        public static byte[] GetBytes(short value)
        {
            return CorrectOrder(BitConverter.GetBytes(value));
        }

        public static byte[] GetBytes(ushort value)
        {
            return CorrectOrder(BitConverter.GetBytes(value));
        }

        public static byte[] GetBytes(int value)
        {
            return CorrectOrder(BitConverter.GetBytes(value));
        }

        public static byte[] GetBytes(uint value)
        {
            return CorrectOrder(BitConverter.GetBytes(value));
        }

        public static byte[] GetBytes(long value)
        {
            return CorrectOrder(BitConverter.GetBytes(value));
        }

        public static byte[] GetBytes(ulong value)
        {
            return CorrectOrder(BitConverter.GetBytes(value));
        }

        private static byte[] CorrectOrder(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
                return Reverse(bytes);
            return bytes;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Reverse(Span<byte> bytes)
        {
            byte[] reversed = new byte[bytes.Length];
            unsafe
            {
                fixed (byte* pSrc = bytes, pDst = reversed)
                {
                    byte* pSrcI = pSrc;
                    byte* pDstI = pDst + bytes.Length - 1;

                    while(pDstI >= pDst)
                    {
                        *pDstI = *pSrcI;
                        pDstI--;
                        pSrcI++;
                    }
                }
            }
            return reversed;
        }
    }
}
