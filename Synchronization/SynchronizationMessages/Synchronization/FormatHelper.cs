using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SynchronizationMessages.Synchronization
{
    public static class FormatHelper
    {
        public static void WriteInt(Stream stream, int value)
        {
            WriteMB32(stream, value);
        }

        public static void WriteLong(Stream stream, long value)
        {
            for (int i = 0; i < 8; i++)
            {
                stream.WriteByte((byte)value);
                value >>= 8;
            }
        }
        public static void WriteGuid(Stream stream, Guid value)
        {

            WriteString(stream, value.ToString());

        }

        public static void WriteString(Stream stream, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteInt(stream, bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static int ReadInt(Stream stream)
        {
            return ReadMB32(stream);
        }

        public static long ReadLong(Stream stream)
        {
            long result = 0;
            for (int i = 0; i < 8; i++)
            {
                long temp = stream.ReadByte();
                result |= temp << (i * 8);
            }

            return result;
        }

        public static string ReadString(Stream stream)
        {
            int size = ReadMB32(stream);
            byte[] buffer = new byte[size];
            stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }
        public static Guid ReadGuid(Stream stream)
        {
            Guid result;
            var guidString = ReadString(stream);
            if (Guid.TryParse(guidString, out result))
                return result;
            return Guid.Empty;
        }
        private static void WriteMB32(Stream stream, int value)
        {
            if (value < 0)
            {
                value = value - int.MinValue; // this will keep odd values odd and even values even
            }

            while ((value & 0xFFFFFF80) != 0)
            {
                stream.WriteByte((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }

            stream.WriteByte((byte)value);
        }

        private static int ReadMB32(Stream stream)
        {
            int result = 0;
            int b;
            int toShift = 0;
            do
            {
                b = stream.ReadByte();
                result |= (b & 0x7F) << toShift;
                toShift += 7;
            } while (b >= 0x80);

            return result;
        }
    }
}
