using System;
using System.Collections.Generic;
using System.Linq;
using WB.Services.Export.Services.Processing.Good;
using WB.Services.Export.Utils;

namespace WB.Services.Export.Services.Storage
{
    public class S3ImageFileStorage : IImageFileStorage
    {
        private readonly IExternalFileStorage externalFileStorage;

        public S3ImageFileStorage(IExternalFileStorage externalFileStorage)
        {
            this.externalFileStorage = externalFileStorage;
        }

        public byte[] GetInterviewBinaryData(Guid interviewId, string filename)
        {
            return this.externalFileStorage.GetBinary(GetPath(interviewId, filename));
        }

        private string GetPath(Guid interviewId, string filename = null) =>
            $"images/{interviewId.FormatGuid()}/{filename ?? String.Empty}";

        public List<InterviewBinaryDataDescriptor> GetBinaryFilesForInterview(Guid interviewId)
        {
            var prefix = GetPath(interviewId);
            var files = this.externalFileStorage.List(prefix);

            return files.Select(file =>
            {
                var filename = file.Path.Substring(prefix.Length);
                return new InterviewBinaryDataDescriptor(interviewId, filename, "image/jpg",
                    () => this.GetInterviewBinaryData(interviewId, GetPath(interviewId, filename)));
            }).ToList();
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
        {
            externalFileStorage.Store(GetPath(interviewId, fileName), data, contentType);
        }

        public void RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            externalFileStorage.Remove(GetPath(interviewId, fileName));
        }
    }
}