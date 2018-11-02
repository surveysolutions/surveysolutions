using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public abstract class InterviewerFileStorage<TMetadataView, TFileView> : IInterviewFileStorage
        where TMetadataView : class, IFileMetadataView, IPlainStorageEntity, new()
        where TFileView : class, IFileView, IPlainStorageEntity, new()
    {
        private readonly IPlainStorage<TMetadataView> fileMetadataViewStorage;
        private readonly IPlainStorage<TFileView> fileViewStorage;
        private readonly IEncryptionService encryptionService;

        protected InterviewerFileStorage(
            IPlainStorage<TMetadataView> fileMetadataViewStorage,
            IPlainStorage<TFileView> fileViewStorage,
            IEncryptionService encryptionService)
        {
            this.fileMetadataViewStorage = fileMetadataViewStorage;
            this.fileViewStorage = fileViewStorage;
            this.encryptionService = encryptionService;
        }

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var metadataView = this.fileMetadataViewStorage.FirstOrDefault(metadata =>
                metadata.InterviewId == interviewId && metadata.FileName == fileName);

            return metadataView == null ? null : this.GetFileById(metadataView.FileId);
        }

        public List<InterviewBinaryDataDescriptor> GetBinaryFilesForInterview(Guid interviewId)
        {
            var metadataViews = this.fileMetadataViewStorage.Where(metadata => metadata.InterviewId == interviewId);
            return metadataViews.Select(m =>
                new InterviewBinaryDataDescriptor(
                    m.InterviewId,
                    m.FileName,
                    m.ContentType,
                    () => this.GetFileById(m.FileId)
                )
            ).ToList();
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType)
        {
            var encryptedData = this.encryptionService.Encrypt(data);
            var metadataView =
                this.fileMetadataViewStorage.FirstOrDefault(metadata => metadata.InterviewId == interviewId && metadata.FileName == fileName);

            if (metadataView == null)
            {
                string fileId = Guid.NewGuid().FormatGuid();
                this.fileViewStorage.Store(new TFileView
                {
                    Id = fileId,
                    File = encryptedData
                });

                this.fileMetadataViewStorage.Store(new TMetadataView
                {
                    Id = Guid.NewGuid().FormatGuid(),
                    InterviewId = interviewId,
                    FileId = fileId,
                    FileName = fileName,
                    ContentType = contentType
                });
            }
            else
            {
                this.fileViewStorage.Store(new TFileView
                {
                    Id = metadataView.FileId,
                    File = encryptedData
                });
            }
        }

        public void RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var metadataView = GetMetadata(interviewId, fileName);

            if (metadataView == null) return;

            this.fileViewStorage.Remove(metadataView.FileId);
            this.fileMetadataViewStorage.Remove(metadataView.Id);
        }

        private TMetadataView GetMetadata(Guid interviewId, string fileName)
        {
            return this.fileMetadataViewStorage
                .FirstOrDefault(metadata => metadata.InterviewId == interviewId && metadata.FileName == fileName);
        }

        private byte[] GetFileById(string fileId)
        {
            var fileView = this.fileViewStorage.GetById(fileId);

            return fileView == null ? null : this.encryptionService.Decrypt(fileView.File);
        }
    }
}
