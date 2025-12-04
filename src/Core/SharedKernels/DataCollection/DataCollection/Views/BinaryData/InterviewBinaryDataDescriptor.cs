using System;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Views.BinaryData
{
    public class InterviewBinaryDataDescriptor
    {
        public InterviewBinaryDataDescriptor(Guid interviewId, string fileName, string contentType, Func<Task<byte[]>> getData, string md5)
        {
            this.InterviewId = interviewId;
            this.FileName = fileName;
            this.getData = getData;
            this.ContentType = contentType;
            this.Md5 = md5;
        }

        public Guid InterviewId { get; private set; }
        public string FileName { get; private set; }
        public string ContentType { get; private set; }
        public string Md5 { get; private set; }

        public async Task<byte[]> GetData()
        {
            return await this.getData();
        }

        private readonly Func<Task<byte[]>> getData;
    }
}
