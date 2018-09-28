using System;
using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public static class StreamExtensions
    {
        public static byte[] ReadExactly(this Stream stream, long seek, long? count)
        {
            if(!stream.CanSeek) throw new ArgumentException(nameof(stream), @"Cannot read non seekable stream");

            stream.Seek(seek, SeekOrigin.Begin);
            var leftToRead = stream.Length - stream.Position; // get amount that left to read after seek
            var wantToRead = count ?? leftToRead;
            var amountToRead = Math.Min(leftToRead, wantToRead);  // make sure that requested length is less then left to red
            int toRead = (int)Math.Min(amountToRead, int.MaxValue);// make sure that amount to read is less then int max value

            var response = stream.ReadExactly(toRead);
            return response;
        }

        public static byte[] ReadExactly(this Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int read = stream.Read(buffer, offset, count - offset);
                if (read == 0)
                    throw new EndOfStreamException();
                offset += read;
            }
            System.Diagnostics.Debug.Assert(offset == count);
            return buffer;
        }
    }
}
