using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class AudioFileStorage : IAudioFileStorage
    {
        private readonly IPlainStorage<AudioFileMetadataView> imageViewStorage;
        private readonly IPlainStorage<AudioFileView> fileViewStorage;

        public AudioFileStorage(
            IPlainStorage<AudioFileMetadataView> imageViewStorage,
            IPlainStorage<AudioFileView> fileViewStorage)
        {
            this.imageViewStorage = imageViewStorage;
            this.fileViewStorage = fileViewStorage;
        }

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var imageView =
                this.imageViewStorage.Where(image => image.InterviewId == interviewId && image.FileName == fileName)
                    .SingleOrDefault();

            if (imageView == null) return null;

            var fileView = this.fileViewStorage.GetById(imageView.FileId);

            return fileView?.Content;
        }

        public List<InterviewBinaryDataDescriptor> GetBinaryFilesForInterview(Guid interviewId)
        {
            throw new NotImplementedException();
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data)
        {
            var imageView =
                this.imageViewStorage.Where(image => image.InterviewId == interviewId && image.FileName == fileName)
                    .SingleOrDefault();

            if (imageView == null)
            {
                string fileId = Guid.NewGuid().FormatGuid();
                this.fileViewStorage.Store(new AudioFileView
                {
                    Id = fileId,
                    Content = data
                });

                this.imageViewStorage.Store(new AudioFileMetadataView
                {
                    Id = Guid.NewGuid().FormatGuid(),
                    InterviewId = interviewId,
                    FileId = fileId,
                    FileName = fileName
                });
            }
            else
            {
                this.fileViewStorage.Store(new AudioFileView
                {
                    Id = imageView.FileId,
                    Content = data
                });
            }
        }

        public void RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var imageView = this.imageViewStorage.Where(image => image.InterviewId == interviewId && image.FileName == fileName).SingleOrDefault();

            if (imageView == null) return;

            this.fileViewStorage.Remove(imageView.FileId);
            this.imageViewStorage.Remove(imageView.Id);
        }
    }
}