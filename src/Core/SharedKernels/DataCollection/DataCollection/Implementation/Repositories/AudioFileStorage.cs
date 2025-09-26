using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    public class AudioFileStorage : IAudioFileStorage
    {
        private readonly IPlainStorageAccessor<AudioFile> filePlainStorageAccessor;

        public AudioFileStorage(IPlainStorageAccessor<AudioFile> filePlainStorageAccessor)
        {
            this.filePlainStorageAccessor = filePlainStorageAccessor;
        }

        public Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName) 
            => Task.FromResult(this.GetInterviewBinaryData(interviewId, fileName));

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var fileId = AudioFile.GetFileId(interviewId, fileName);
            var databaseFile = filePlainStorageAccessor.GetById(fileId);

            return databaseFile?.Data;
        }

        public Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
        {
            var databaseFiles = filePlainStorageAccessor.Query(q => q.Where(f => f.InterviewId == interviewId));

            var interviewBinaryDataDescriptors = databaseFiles.Select(file 
                => new InterviewBinaryDataDescriptor(
                    interviewId, 
                    file.FileName,
                    file.ContentType,
                    () => GetInterviewBinaryDataAsync(interviewId, file.FileName),
                    null
                )).ToList();
            return Task.FromResult(interviewBinaryDataDescriptors);
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
        {
            var file = new AudioFile()
            {
                Id = AudioFile.GetFileId(interviewId, fileName),
                InterviewId = interviewId,
                FileName = fileName,
                Data = data,
                ContentType = contentType
            };
            var fileId = AudioFile.GetFileId(interviewId, fileName);

            filePlainStorageAccessor.Store(file, fileId);
        }

        public Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var fileId = AudioFile.GetFileId(interviewId, fileName);
            filePlainStorageAccessor.Remove(fileId);
            return Task.CompletedTask;
        }
    }
}
