using System;
using System.Threading.Tasks;
using SQLite;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class InterviewerAudioAuditFileStorage : InterviewerFileStorage<AudioAuditView, AudioAuditFileView>, IAudioAuditFileStorage
    {
        public InterviewerAudioAuditFileStorage(
            IPlainStorage<AudioAuditView> imageViewStorage,
            IPlainStorage<AudioAuditFileView> fileViewStorage,
            IEncryptionService encryptionService)
            : base(imageViewStorage, fileViewStorage, encryptionService)
        {
        }
        
        public Task<bool> HasAnyAudioAuditFilesStoredAsync(QuestionnaireIdentity questionnaire)
        {
            throw new NotImplementedException();
        }
    }

    public class AudioAuditView : IFileMetadataView, IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public Guid InterviewId { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }

    public class AudioAuditFileView : IFileView, IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public byte[] File { get; set; }
    }
}
