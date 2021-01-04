using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public class AudioAuditFileS3Storage : AudioAuditStorageBase
    {
        private const string AudioAuditS3Folder = "audio_audit/";
        private readonly IExternalFileStorage externalFileStorage;
        private readonly IPlainStorageAccessor<AudioAuditFile> filePlainStorageAccessor;

        public AudioAuditFileS3Storage(
            IExternalFileStorage externalFileStorage,
            IPlainStorageAccessor<AudioAuditFile> filePlainStorageAccessor,
            IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            this.externalFileStorage = externalFileStorage ?? throw new ArgumentNullException(nameof(externalFileStorage));
            this.filePlainStorageAccessor = filePlainStorageAccessor ?? throw new ArgumentNullException(nameof(filePlainStorageAccessor));
        }

        public override async Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName)
        {
            string fileId = AudioAuditFile.GetFileId(interviewId, fileName);
            var audioAuditData = filePlainStorageAccessor.GetById(fileId);
            if (audioAuditData.Data == null)
            {
                return await externalFileStorage.GetBinaryAsync(AudioAuditS3Folder + fileId);
            }
            return audioAuditData.Data;
        }

        public override byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            throw new NotImplementedException();
        }

        public override Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId)
        {
            var databaseFiles = filePlainStorageAccessor.Query(q => q.Where(f => f.InterviewId == interviewId));

            var interviewBinaryDataDescriptors = databaseFiles.Select(file
                => new InterviewBinaryDataDescriptor(
                    interviewId,
                    file.FileName,
                    file.ContentType,
                    () => GetInterviewBinaryDataAsync(interviewId, file.FileName)
                )).ToList();
            return Task.FromResult(interviewBinaryDataDescriptors);
        }

        public override void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
        {
            var id = AudioAuditFile.GetFileId(interviewId, fileName);
            var file = new AudioAuditFile
            {
                Id = id,
                InterviewId = interviewId,
                FileName = fileName,
                ContentType = contentType
            };
            filePlainStorageAccessor.Store(file, file.Id);
            externalFileStorage.Store(AudioAuditS3Folder + id, data, contentType);
        }

        public override async Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var fileId = AudioAuditFile.GetFileId(interviewId, fileName);
            filePlainStorageAccessor.Remove(fileId);
            await externalFileStorage.RemoveAsync(AudioAuditS3Folder + fileId);
        }
    }
}
