using System;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Views.BinaryData
{
    public class InterviewBinaryDataDescriptor
    {
        public InterviewBinaryDataDescriptor(Guid interviewId, string fileName, string contentType, Func<Task<byte[]>> getData)
        {
            this.InterviewId = interviewId;
            this.FileName = fileName;
            this.getData = getData;
            this.ContentType = contentType;
        }

        public Guid InterviewId { get; private set; }
        public string FileName { get; private set; }
        public string ContentType { get; private set; }

        public async Task<byte[]> GetData()
        {
            return await this.getData();
        }

        private readonly Func<Task<byte[]>> getData;
    }
}
