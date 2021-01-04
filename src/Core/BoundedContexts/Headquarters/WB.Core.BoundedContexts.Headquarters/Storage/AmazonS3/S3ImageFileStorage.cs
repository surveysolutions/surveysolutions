using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3
{
    public class S3ImageFileStorage : IImageFileStorage
    {
        private readonly IExternalFileStorage externalFileStorage;

        public S3ImageFileStorage(IExternalFileStorage externalFileStorage)
        {
            this.externalFileStorage = externalFileStorage;
        }

        public Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string filename)
        {
            return this.externalFileStorage.GetBinaryAsync(GetPath(interviewId, filename));
        }

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            return GetInterviewBinaryDataAsync(interviewId, fileName).Result;
        }

        public string GetPath(Guid interviewId, string filename = null) => $"{GetInterviewDirectoryPath(interviewId)}/{filename ?? String.Empty}";
        private string GetInterviewDirectoryPath(Guid interviewId) => $"images/{interviewId.FormatGuid()}";
        public async Task RemoveAllBinaryDataForInterviewsAsync(List<Guid> interviewIds)
        {
            if (!interviewIds.Any()) return;

            var paths = interviewIds.Select(id => GetInterviewDirectoryPath(id));
            await externalFileStorage.RemoveAsync(paths).ConfigureAwait(false);
        }

        public async Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
        {
            var prefix = GetPath(interviewId);
            var files = await this.externalFileStorage.ListAsync(prefix).ConfigureAwait(false);

            return files.Select(file =>
            {
                var filename = file.Path.Substring(prefix.Length);
                return new InterviewBinaryDataDescriptor(interviewId, filename, "image/jpg",
                    () => this.GetInterviewBinaryDataAsync(interviewId, filename));
            }).ToList();
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
        {
            externalFileStorage.Store(GetPath(interviewId, fileName), data, contentType);
        }

        public async Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            await externalFileStorage.RemoveAsync(GetPath(interviewId, fileName)).ConfigureAwait(false);
        }
    }
}
