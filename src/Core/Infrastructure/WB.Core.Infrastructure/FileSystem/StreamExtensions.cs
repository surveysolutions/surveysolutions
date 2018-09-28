using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public static class StreamExtensions
    {
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
