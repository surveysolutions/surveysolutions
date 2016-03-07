using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Repositories
{
    public class QuestionnaireAttachmentStorage : IQuestionnaireAttachmentStorage
    {
        private readonly IPlainKeyValueStorage<Attachment> repository;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string attachmentDirectoryPath;

        private readonly string AttachmentDirectoryName = "Attachments";

        public QuestionnaireAttachmentStorage(IPlainKeyValueStorage<Attachment> repository,
            IFileSystemAccessor fileSystemAccessor, string rootDirectoryPath)
        {
            this.repository = repository;
            this.fileSystemAccessor = fileSystemAccessor;

            this.attachmentDirectoryPath = this.fileSystemAccessor.CombinePath(rootDirectoryPath, AttachmentDirectoryName);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.attachmentDirectoryPath))
                this.fileSystemAccessor.CreateDirectory(this.attachmentDirectoryPath);
        }

        public Task StoreAsync(Attachment attachment, byte[] attachmentData)
        {
            this.repository.Store(attachment, attachment.Id);

            var attachmentFilePath = this.GetPathToFile(attachment.Id);
            fileSystemAccessor.WriteAllBytes(attachmentFilePath, attachmentData);
            return Task.FromResult(true);
        }

        public Task<Attachment> GetAttachmentAsync(string attachmentId)
        {
            return Task.FromResult(this.repository.GetById(attachmentId));
        }

        public Task<byte[]> GetAttachmentContentAsync(string attachmentId)
        {
            var filePath = this.GetPathToFile(attachmentId);
            if (!fileSystemAccessor.IsFileExists(filePath))
                return null;
            return Task.FromResult(fileSystemAccessor.ReadAllBytes(filePath));
        }

        private string GetPathToFile(string fileName)
        {
            return fileSystemAccessor.CombinePath(attachmentDirectoryPath, fileName);
        }
    }
}