using System;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    public class BinaryData
    {
        public Guid InterviewId { get; set; }
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public BinaryDataType Type { get; set; }
    }

    public enum BinaryDataType
    {
        Image = 1,
        Audio,
        AudioAudit
    }
}
