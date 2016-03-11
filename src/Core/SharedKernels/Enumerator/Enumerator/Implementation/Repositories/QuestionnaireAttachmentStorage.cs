using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class QuestionnaireAttachmentStorage : IQuestionnaireAttachmentStorage
    {
        private readonly IAsyncPlainStorage<AttachmentMetadata> repository;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string attachmentDirectoryPath;

        private readonly string AttachmentDirectoryName = "Attachments";

        public QuestionnaireAttachmentStorage(IAsyncPlainStorage<AttachmentMetadata> repository,
            IFileSystemAccessor fileSystemAccessor, string rootDirectoryPath)
        {
            this.repository = repository;
            this.fileSystemAccessor = fileSystemAccessor;

            this.attachmentDirectoryPath = this.fileSystemAccessor.CombinePath(rootDirectoryPath, this.AttachmentDirectoryName);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.attachmentDirectoryPath))
                this.fileSystemAccessor.CreateDirectory(this.attachmentDirectoryPath);
        }

        public Task StoreAttachmentContentAsync(string attachmentId, byte[] attachmentData)
        {
            var attachmentFilePath = this.GetPathToFile(attachmentId);
            this.fileSystemAccessor.WriteAllBytes(attachmentFilePath, attachmentData);
            return Task.FromResult(true);
        }

        public async Task StoreAsync(AttachmentMetadata attachmentMetadata)
        {
            await this.repository.StoreAsync(attachmentMetadata);
        }
        
        public Task<AttachmentMetadata> GetAttachmentAsync(string attachmentId)
        {
            return Task.FromResult(this.repository.GetById(attachmentId));
        }

        public Task<byte[]> GetAttachmentContentAsync(string attachmentId)
        {
            var filePath = this.GetPathToFile(attachmentId);
            if (!this.fileSystemAccessor.IsFileExists(filePath))
                return null;
            return Task.FromResult(this.fileSystemAccessor.ReadAllBytes(filePath));
        }

        public Task<bool> IsExistAttachmentContentAsync(string attachmentId)
        {
            var attachmentFilePath = this.GetPathToFile(attachmentId);
            var fileExists = this.fileSystemAccessor.IsFileExists(attachmentFilePath);
            return Task.FromResult(fileExists);
        }

        private string GetPathToFile(string fileName)
        {
            return this.fileSystemAccessor.CombinePath(this.attachmentDirectoryPath, fileName);
        }
    }
}