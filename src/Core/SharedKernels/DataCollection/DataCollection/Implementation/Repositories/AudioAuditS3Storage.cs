using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    public class AudioAuditFileS3Storage : IAudioAuditFileStorage
    {
        private const string AudioAuditS3Folder = "audio_audit/";
        private readonly IExternalFileStorage externalFileStorage;
        private readonly IPlainStorageAccessor<AudioAuditFile> filePlainStorageAccessor;

        public AudioAuditFileS3Storage(IExternalFileStorage externalFileStorage, 
            IPlainStorageAccessor<AudioAuditFile> filePlainStorageAccessor)
        {
            this.externalFileStorage = externalFileStorage ?? throw new ArgumentNullException(nameof(externalFileStorage));
            this.filePlainStorageAccessor = filePlainStorageAccessor ?? throw new ArgumentNullException(nameof(filePlainStorageAccessor));
        }

        public async Task<byte[]> GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            string fileId = AudioAuditFile.GetFileId(interviewId, fileName);
            var audioAuditData = filePlainStorageAccessor.GetById(fileId);
            if (audioAuditData.Data == null)
            {
                var databaseFile = await externalFileStorage.GetBinaryAsync(AudioAuditS3Folder + fileId);
                return databaseFile;
            }
            return audioAuditData.Data;
        }

        public Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
        {
            var databaseFiles = filePlainStorageAccessor.Query(q => q.Where(f => f.InterviewId == interviewId));

            var interviewBinaryDataDescriptors = databaseFiles.Select(file
                => new InterviewBinaryDataDescriptor(
                    interviewId,
                    file.FileName,
                    file.ContentType,
                    () => GetInterviewBinaryData(interviewId, file.FileName)
                )).ToList();
            return Task.FromResult(interviewBinaryDataDescriptors);
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
        {
            var id = AudioAuditFile.GetFileId(interviewId, fileName);
            var file = new AudioAuditFile
            {
                Id = id,
                InterviewId = interviewId,
                FileName = fileName,
                ContentType = contentType
            };
            this.filePlainStorageAccessor.Store(file, file.Id);
            this.externalFileStorage.Store(AudioAuditS3Folder + id, data, contentType);
        }

        public async Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var fileId = AudioAuditFile.GetFileId(interviewId, fileName);
            this.filePlainStorageAccessor.Remove(fileId);
            await this.externalFileStorage.RemoveAsync(AudioAuditS3Folder + fileId);
        }
    }
}
