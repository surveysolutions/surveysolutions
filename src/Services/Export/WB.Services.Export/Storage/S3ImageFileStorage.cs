using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Services.Infrastructure;

namespace WB.Services.Export.Storage
{
    public class S3ImageFileStorage : IImageFileStorage
    {
        private readonly IExternalArtifactsStorage externalArtifactsStorage;

        public S3ImageFileStorage(IExternalArtifactsStorage externalArtifactsStorage)
        {
            this.externalArtifactsStorage = externalArtifactsStorage;
        }

        public Task<byte[]?> GetInterviewBinaryData(Guid interviewId, string filename)
        {
            return this.externalArtifactsStorage.GetBinaryAsync(GetPath(interviewId, filename));
        }

        private string GetPath(Guid interviewId, string? filename = null) =>
            $"images/{interviewId.FormatGuid()}/{filename ?? String.Empty}";

        public async Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
        {
            var prefix = GetPath(interviewId);
            var files = await this.externalArtifactsStorage.ListAsync(prefix);

            if (files == null)
                return new List<InterviewBinaryDataDescriptor>();
            
            return files.Select(file =>
            {
                var filename = file.Path.Substring(prefix.Length);
                return new InterviewBinaryDataDescriptor(interviewId, filename, "image/jpg",
                    () => this.GetInterviewBinaryData(interviewId, GetPath(interviewId, filename)));
            }).ToList();
        }

        public Task StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
        {
            return externalArtifactsStorage.StoreAsync(GetPath(interviewId, fileName), data, contentType);
        }

        public Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            return externalArtifactsStorage.RemoveAsync(GetPath(interviewId, fileName));
        }
    }
}
