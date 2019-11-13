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

        public Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName) 
            => Task.FromResult(GetInterviewBinaryData(interviewId, fileName));

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName) 
            => this.mediaStorage.Get(fileName, interviewId)?.Data;

        public Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
        {
            throw new NotImplementedException();
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
    }
}
