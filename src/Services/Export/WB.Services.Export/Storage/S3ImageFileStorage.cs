using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Storage
{
    public class S3ImageFileStorage : IImageFileStorage
    {
        private readonly IExternalFileStorage externalFileStorage;

        public S3ImageFileStorage(IExternalFileStorage externalFileStorage)
        {
            this.externalFileStorage = externalFileStorage;
        }

        public Task<byte[]> GetInterviewBinaryData(Guid interviewId, string filename)
        {
            return this.externalFileStorage.GetBinaryAsync(GetPath(interviewId, filename));
        }

        private string GetPath(Guid interviewId, string filename = null) =>
            $"images/{interviewId.FormatGuid()}/{filename ?? String.Empty}";

        public async Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
        {
            var prefix = GetPath(interviewId);
            var files = await this.externalFileStorage.ListAsync(prefix);

            return files.Select(file =>
            {
                var filename = file.Path.Substring(prefix.Length);
                return new InterviewBinaryDataDescriptor(interviewId, filename, "image/jpg",
                    () => this.GetInterviewBinaryData(interviewId, GetPath(interviewId, filename)));
            }).ToList();
        }

        public Task StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
        {
            return externalFileStorage.StoreAsync(GetPath(interviewId, fileName), data, contentType);
        }

        public Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            return externalFileStorage.RemoveAsync(GetPath(interviewId, fileName));
        }
    }
}
