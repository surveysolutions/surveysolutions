using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Repositories
{
    public class AudioAuditFileStorage : AudioAuditStorageBase
    {
        private readonly IPlainStorageAccessor<AudioAuditFile> filePlainStorageAccessor;

        public AudioAuditFileStorage(
            IPlainStorageAccessor<AudioAuditFile> filePlainStorageAccessor,
            IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            this.filePlainStorageAccessor = filePlainStorageAccessor;
        }

        public override Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName)
            => Task.FromResult(GetInterviewBinaryData(interviewId, fileName));

        public override byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var fileId = AudioAuditFile.GetFileId(interviewId, fileName);
            var databaseFile = filePlainStorageAccessor.GetById(fileId);

            return databaseFile?.Data;
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
            var file = new AudioAuditFile()
            {
                Id = AudioAuditFile.GetFileId(interviewId, fileName),
                InterviewId = interviewId,
                FileName = fileName,
                Data = data,
                ContentType = contentType
            };
            var fileId = AudioAuditFile.GetFileId(interviewId, fileName);

            filePlainStorageAccessor.Store(file, fileId);
        }

        public override Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var fileId = AudioAuditFile.GetFileId(interviewId, fileName);
            filePlainStorageAccessor.Remove(fileId);
            return Task.CompletedTask;
        }
    }
}
