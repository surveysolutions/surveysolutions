using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    public class AudioAuditFile : IView
    {
        public virtual string Id { get; set; }

        public virtual Guid InterviewId { get; set; }

        public virtual string FileName { get; set; }

        public virtual byte[] Data { get; set; }

        public virtual string ContentType { get; set; }

        public static string GetFileId(Guid interviewId, string fileName) => $"{interviewId}#{fileName}";
    }

    public class AudioAuditFileStorage : IAudioAuditFileStorage
    {
        private readonly IPlainStorageAccessor<AudioAuditFile> filePlainStorageAccessor;

        public AudioAuditFileStorage(IPlainStorageAccessor<AudioAuditFile> filePlainStorageAccessor)
        {
            this.filePlainStorageAccessor = filePlainStorageAccessor;
        }

        public Task<byte[]> GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var fileId = AudioAuditFile.GetFileId(interviewId, fileName);
            var databaseFile = filePlainStorageAccessor.GetById(fileId);
            return Task.FromResult(databaseFile?.Data);
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

        public Task RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var fileId = AudioAuditFile.GetFileId(interviewId, fileName);
            filePlainStorageAccessor.Remove(fileId);
            return Task.CompletedTask;
        }
    }
}
