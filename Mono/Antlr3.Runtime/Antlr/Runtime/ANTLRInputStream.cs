namespace Antlr.Runtime
{
    using System;
    using System.IO;
    using System.Text;

    public class ANTLRInputStream : ANTLRReaderStream
    {
        protected ANTLRInputStream()
        {
        }

        public ANTLRInputStream(Stream istream) : this(istream, (Encoding) null)
        {
        }

        public ANTLRInputStream(Stream istream, int size) : this(istream, size, null)
        {
        }

        public ANTLRInputStream(Stream istream, Encoding encoding) : this(istream, ANTLRReaderStream.INITIAL_BUFFER_SIZE, encoding)
        {
        }

        public ANTLRInputStream(Stream istream, int size, Encoding encoding) : this(istream, size, ANTLRReaderStream.READ_BUFFER_SIZE, encoding)
        {
        }

        public ANTLRInputStream(Stream istream, int size, int readBufferSize, Encoding encoding)
        {
            StreamReader reader;
            if (encoding != null)
            {
                reader = new StreamReader(istream, encoding);
            }
            else
            {
                reader = new StreamReader(istream);
            }
            this.Load(reader, size, readBufferSize);
        }
    }
}

