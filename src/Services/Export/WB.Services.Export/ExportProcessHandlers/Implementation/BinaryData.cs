using System;
using System.IO;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    public class BinaryData
    {
        public Guid InterviewId { get; set; }
        public string FileName { get; set; } = String.Empty;
        public Stream Content { get; set; } = null!;
        public BinaryDataType Type { get; set; }
        public long ContentLength { get; set; }
        public string InterviewKey { get; set; } = String.Empty;
    }
}
