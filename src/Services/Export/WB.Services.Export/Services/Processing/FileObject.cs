using System;

namespace WB.Services.Export.Services.Processing
{
    public class FileObject
    {
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
    }
}
