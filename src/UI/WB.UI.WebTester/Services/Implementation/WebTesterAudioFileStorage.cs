using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.UI.WebTester.Services.Implementation
{
    public class WebTesterAudioFileStorage : IAudioFileStorage
    {
        private readonly ICacheStorage<MultimediaFile, string> mediaStorage;

        public WebTesterAudioFileStorage(ICacheStorage<MultimediaFile, string> mediaStorage)
        {
            this.mediaStorage = mediaStorage;
        }

        byte[] IInterviewFileStorage.GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName)
        {
            var mediaFile = this.mediaStorage.Get(fileName, interviewId);
            if (mediaFile == null)
                throw new InvalidOperationException("Media file must not be null.");


            return Task.FromResult(mediaFile.Data);
        }

        public Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
        {
            throw new NotImplementedException();
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
        {
            var file = new MultimediaFile(fileName, data, null, contentType);
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
    }
}
