namespace Antlr.Runtime
{
    using System;
    using System.IO;

    public class ANTLRReaderStream : ANTLRStringStream
    {
        public static readonly int INITIAL_BUFFER_SIZE = 0x400;
        public static readonly int READ_BUFFER_SIZE = 0x400;

        protected ANTLRReaderStream()
        {
        }

        public ANTLRReaderStream(TextReader reader) : this(reader, INITIAL_BUFFER_SIZE, READ_BUFFER_SIZE)
        {
        }

        public ANTLRReaderStream(TextReader reader, int size) : this(reader, size, READ_BUFFER_SIZE)
        {
        }

        public ANTLRReaderStream(TextReader reader, int size, int readChunkSize)
        {
            this.Load(reader, size, readChunkSize);
        }

        public virtual void Load(TextReader reader, int size, int readChunkSize)
        {
            if (reader != null)
            {
                if (size <= 0)
                {
                    size = INITIAL_BUFFER_SIZE;
                }
                if (readChunkSize <= 0)
                {
                    readChunkSize = READ_BUFFER_SIZE;
                }
                try
                {
                    base.data = new char[size];
                    int num = 0;
                    int index = 0;
                    do
                    {
                        if ((index + readChunkSize) > base.data.Length)
                        {
                            char[] destinationArray = new char[base.data.Length * 2];
                            Array.Copy(base.data, 0, destinationArray, 0, base.data.Length);
                            base.data = destinationArray;
                        }
                        num = reader.Read(base.data, index, readChunkSize);
                        index += num;
                    }
                    while (num != 0);
                    base.n = index;
                }
                finally
                {
                    reader.Close();
                }
            }
        }
    }
}

