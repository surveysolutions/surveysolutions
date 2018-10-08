using System;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    public class BinaryData
    {
        public Guid InterviewId { get; set; }
        public string Answer { get; set; }
        public byte[] Content { get; set; }
    }
}