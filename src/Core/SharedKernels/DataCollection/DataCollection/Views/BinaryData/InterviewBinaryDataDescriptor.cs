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
            Func<Task<Stream>> streamAccessor = getStream;
            if (streamAccessor == null)
            {
                streamAccessor = async () =>
                {
                    var data = await getData();
                    return data == null ? null : new MemoryStream(data, writable: false);
                };
            }

            Func<Task<byte[]>> dataAccessor = getData;
            if (dataAccessor == null)
            {
                dataAccessor = async () =>
                {
                    using var stream = await streamAccessor();
                    if (stream == null)
                        return null;

                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                };
            }

            this.InterviewId = interviewId;
            this.FileName = fileName;
            this.getData = dataAccessor;
            this.ContentType = contentType;
            this.Md5 = md5;
            this.getStream = streamAccessor;
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
            return await this.getStream();
        }

        private readonly Func<Task<byte[]>> getData;
        private readonly Func<Task<Stream>> getStream;
    }
}
