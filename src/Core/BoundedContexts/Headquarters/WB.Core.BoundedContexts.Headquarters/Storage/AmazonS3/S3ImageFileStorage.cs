﻿using System;
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
            throw new NotImplementedException();
        }

        public string GetPath(Guid interviewId, string filename = null) => $"images/{interviewId.FormatGuid()}/{filename ?? String.Empty}";

        public async Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
        {
            var prefix = GetPath(interviewId);
            var files = await this.externalFileStorage.ListAsync(prefix).ConfigureAwait(false);

            return files.Select(file =>
            {
                var filename = file.Path.Substring(prefix.Length);
                return new InterviewBinaryDataDescriptor(interviewId, filename, "image/jpg",
                    () => this.GetInterviewBinaryDataAsync(interviewId, GetPath(interviewId, filename)));
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
