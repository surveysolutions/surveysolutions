using System;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    public class BinaryData
    {
        public Guid InterviewId { get; set; }
        public string FileName { get; set; }
        public byte[] Content { get; set; }
    }
}
