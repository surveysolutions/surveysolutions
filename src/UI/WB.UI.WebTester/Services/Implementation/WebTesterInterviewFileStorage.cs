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
            => Task.FromResult(this.GetInterviewBinaryData(interviewId, fileName));

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName) 
            => this.mediaStorage.Get(fileName, interviewId)?.Data;

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
            mediaStorage.Store(new MultimediaFile
            {
                Filename = fileName,
                Data = data,
                MimeType = contentType
            }, fileName, interviewId);
        }

        public Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            mediaStorage.Remove(fileName, interviewId);
            return Task.CompletedTask;
        }

        public string GetPath(Guid interviewId, string filename = null)
        {
            throw new NotImplementedException();
        }
    }
}
