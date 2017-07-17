using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;


namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public abstract class InterviewerFileStorage<TMetadataView, TFileView> : IInterviewFileStorage
        where TMetadataView : class, IFileMetadataView, IPlainStorageEntity, new()
        where TFileView : class, IFileView, IPlainStorageEntity, new()
    {
        private readonly IPlainStorage<TMetadataView> fileMetadataViewStorage;
        private readonly IPlainStorage<TFileView> fileViewStorage;

        protected InterviewerFileStorage(
            IPlainStorage<TMetadataView> fileMetadataViewStorage,
            IPlainStorage<TFileView> fileViewStorage)
        {
            this.fileMetadataViewStorage = fileMetadataViewStorage;
            this.fileViewStorage = fileViewStorage;
        }

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var imageView =
                this.fileMetadataViewStorage.Where(image => image.InterviewId == interviewId && image.FileName == fileName)
                    .SingleOrDefault();

            if (imageView == null) return null;

            var fileView = this.fileViewStorage.GetById(imageView.FileId);

            return fileView?.File;
        }

        public List<InterviewBinaryDataDescriptor> GetBinaryFilesForInterview(Guid interviewId)
        {
            throw new NotImplementedException();
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data)
        {
            var imageView =
                this.fileMetadataViewStorage.Where(image => image.InterviewId == interviewId && image.FileName == fileName)
                    .SingleOrDefault();

            if (imageView == null)
            {
                string fileId = Guid.NewGuid().FormatGuid();
                this.fileViewStorage.Store(new TFileView
                {
                    Id = fileId,
                    File = data
                });

                this.fileMetadataViewStorage.Store(new TMetadataView
                {
                    Id = Guid.NewGuid().FormatGuid(),
                    InterviewId = interviewId,
                    FileId = fileId,
                    FileName = fileName
                });
            }
            else
            {
                this.fileViewStorage.Store(new TFileView
                {
                    Id = imageView.FileId,
                    File = data
                });
            }
        }

        public void RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var imageView = this.fileMetadataViewStorage.Where(image => image.InterviewId == interviewId && image.FileName == fileName).SingleOrDefault();

            if (imageView == null) return;

            this.fileViewStorage.Remove(imageView.FileId);
            this.fileMetadataViewStorage.Remove(imageView.Id);
        }
    }
}