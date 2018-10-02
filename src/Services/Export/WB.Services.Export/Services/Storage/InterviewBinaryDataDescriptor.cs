using System;
using System.Threading.Tasks;

namespace WB.Services.Export.Services.Storage
{
    public class InterviewBinaryDataDescriptor
    {
        public InterviewBinaryDataDescriptor(Guid interviewId, string fileName, string contentType,
            Func<Task<byte[]>> getData)
        {
            this.InterviewId = interviewId;
            this.FileName = fileName;
            this.getData = getData;
            this.ContentType = contentType;
        }

        public Guid InterviewId { get; private set; }
        public string FileName { get; private set; }
        public string ContentType { get; private set; }

        public Task<byte[]> GetData()
        {
            return this.getData();
        }

        private readonly Func<Task<byte[]>> getData;
    }
}
