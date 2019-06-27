using System;
using System.IO;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    public class BinaryData
    {
        public Guid InterviewId { get; set; }
        public string FileName { get; set; }
        public Stream Content { get; set; }
        public BinaryDataType Type { get; set; }
        public long ContentLength { get; set; }
    }
}
