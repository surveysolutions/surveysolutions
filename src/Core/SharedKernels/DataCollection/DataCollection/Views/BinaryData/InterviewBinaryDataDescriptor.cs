using System;
using System.IO;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Views.BinaryData
{
    public class InterviewBinaryDataDescriptor
    {
        public InterviewBinaryDataDescriptor(Guid interviewId, string fileName, string contentType, Func<Task<byte[]>> getData, string md5)
            : this(interviewId, fileName, contentType, getData, md5, null)
        {
        }

        public InterviewBinaryDataDescriptor(Guid interviewId, string fileName, string contentType, Func<Task<byte[]>> getData, string md5, Func<Task<Stream>> getStream)
        {
            this.InterviewId = interviewId;
            this.FileName = fileName;
            this.getData = getData;
            this.ContentType = contentType;
            this.Md5 = md5;
            this.getStream = getStream;
        }

        public Guid InterviewId { get; private set; }
        public string FileName { get; private set; }
        public string ContentType { get; private set; }
        public string Md5 { get; private set; }

        public async Task<byte[]> GetData()
        {
            return await this.getData();
        }

        public async Task<Stream> GetStream()
        {
            if (this.getStream != null)
                return await this.getStream();

            var data = await this.getData();
            return data == null ? null : new MemoryStream(data, writable: false);
        }

        private readonly Func<Task<byte[]>> getData;
        private readonly Func<Task<Stream>> getStream;
    }
}
