using System.IO;

namespace WB.Core.Infrastructure.FileSystem
{
    public class ExtractedStream
    {
        public string Name { get; set; }
        public Stream Content { get; set; }
        public long Size { get; set; }
    }
}