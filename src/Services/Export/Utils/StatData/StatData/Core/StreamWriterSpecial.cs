using System.Text;
using System.IO;

namespace StatData.Core
{
    internal class StreamWriterSpecial : BinaryWriter
    {
        internal StreamWriterSpecial(Stream s) : base(s) { }

        /// <summary>
        /// Writes a string to the stream without leading length declaration
        /// </summary>
        /// <param name="s">String to be written</param>
        public void WriteStr(string s)
        {
            Write(Encoding.UTF8.GetBytes(s));
        }
    }
}
