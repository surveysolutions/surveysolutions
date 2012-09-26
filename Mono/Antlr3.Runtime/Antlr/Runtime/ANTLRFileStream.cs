namespace Antlr.Runtime
{
    using System;
    using System.IO;
    using System.Text;

    public class ANTLRFileStream : ANTLRStringStream
    {
        protected string fileName;

        protected ANTLRFileStream()
        {
        }

        public ANTLRFileStream(string fileName) : this(fileName, Encoding.Default)
        {
        }

        public ANTLRFileStream(string fileName, Encoding encoding)
        {
            this.fileName = fileName;
            this.Load(fileName, encoding);
        }

        private long GetFileLength(FileInfo file)
        {
            if (file.Exists)
            {
                return file.Length;
            }
            return 0L;
        }

        public virtual void Load(string fileName, Encoding encoding)
        {
            if (fileName != null)
            {
                StreamReader reader = null;
                try
                {
                    FileInfo file = new FileInfo(fileName);
                    int fileLength = (int) this.GetFileLength(file);
                    base.data = new char[fileLength];
                    if (encoding != null)
                    {
                        reader = new StreamReader(fileName, encoding);
                    }
                    else
                    {
                        reader = new StreamReader(fileName, Encoding.Default);
                    }
                    base.n = reader.Read(base.data, 0, base.data.Length);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }
        }

        public override string SourceName
        {
            get
            {
                return this.fileName;
            }
        }
    }
}

