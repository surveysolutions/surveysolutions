using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.UI.WebTester.Services.Implementation
{
    public class WebTesterImageFileStorage : IImageFileStorage
    {
        private readonly ICacheStorage<MultimediaFile, string> mediaStorage;

        public WebTesterImageFileStorage(ICacheStorage<MultimediaFile, string> mediaStorage)
        {
            this.mediaStorage = mediaStorage;
        }

        public Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName)
        {
            var file = this.mediaStorage.Get(fileName, interviewId);

            if (file == null)
                throw new InvalidOperationException("File must not be null.");

            return Task.FromResult(file.Data);
        }

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var file = this.mediaStorage.Get(fileName, interviewId);

            if(file == null)
                throw new InvalidOperationException("File must not be null.");

            return file.Data;
        }

        public Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
        {
            var interviewBinaryDataDescriptors = this.mediaStorage.GetArea(interviewId).Select(x =>
                    new InterviewBinaryDataDescriptor(interviewId, x.Filename, x.MimeType,
                        () => Task.FromResult(x.Data)))
                .ToList();
            return Task.FromResult(interviewBinaryDataDescriptors);
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
        {
            var file = new MultimediaFile(fileName, data, null,contentType);
            mediaStorage.Store(file, fileName, interviewId);
        }

        public void StoreBrokenInterviewBinaryData(Guid userId, Guid interviewId, string fileName, byte[] data, string contentType)
        {
            throw new NotImplementedException();
        }

        public Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            mediaStorage.Remove(fileName, interviewId);
            return Task.CompletedTask;
        }

        public string GetPath(Guid interviewId, string? filename = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllBinaryDataForInterviewsAsync(List<Guid> interviewIds)
        {
            throw new NotImplementedException();
        }
    }
}
