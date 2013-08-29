namespace SynchronizationMessages.Synchronization
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// The format helper.
    /// </summary>
    public static class FormatHelper
    {
        #region Public Methods and Operators

        /// <summary>
        /// The read guid.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The System.Guid.
        /// </returns>
        public static Guid ReadGuid(Stream stream)
        {
            Guid result;
            string guidString = ReadString(stream);
            if (Guid.TryParse(guidString, out result))
            {
                return result;
            }

            return Guid.Empty;
        }

        /// <summary>
        /// The read int.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
        public static int ReadInt(Stream stream)
        {
            return ReadMB32(stream);
        }

        /// <summary>
        /// The read long.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The System.Int64.
        /// </returns>
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

        /// <summary>
        /// The read string.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        public static string ReadString(Stream stream)
        {
            int size = ReadMB32(stream);
            var buffer = new byte[size];
            stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// The write guid.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public static void WriteGuid(Stream stream, Guid value)
        {
            WriteString(stream, value.ToString());
        }

        /// <summary>
        /// The write int.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public static void WriteInt(Stream stream, int value)
        {
            WriteMB32(stream, value);
        }

        /// <summary>
        /// The write long.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public static void WriteLong(Stream stream, long value)
        {
            for (int i = 0; i < 8; i++)
            {
                stream.WriteByte((byte)value);
                value >>= 8;
            }
        }

        /// <summary>
        /// The write string.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public static void WriteString(Stream stream, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteInt(stream, bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The read m b 32.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The System.Int32.
        /// </returns>
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
            }
            while (b >= 0x80);

            return result;
        }

        /// <summary>
        /// The write m b 32.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
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

        #endregion
    }
}