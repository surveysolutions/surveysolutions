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
    public class InterviewerAudioFileStorage : IAudioFileStorage
    {
        private readonly IPlainStorage<AudioFileMetadataView> audioFileMetadataViewStorage;
        private readonly IPlainStorage<AudioFileView> audioFileViewStorage;

        public InterviewerAudioFileStorage(
            IPlainStorage<AudioFileMetadataView> audioFileMetadataViewStorage,
            IPlainStorage<AudioFileView> audioFileViewStorage)
        {
            this.audioFileMetadataViewStorage = audioFileMetadataViewStorage;
            this.audioFileViewStorage = audioFileViewStorage;
        }

        public byte[] GetInterviewBinaryData(Guid interviewId, string fileName)
        {
            var imageView =
                this.audioFileMetadataViewStorage.Where(image => image.InterviewId == interviewId && image.FileName == fileName)
                    .SingleOrDefault();

            if (imageView == null) return null;

            var fileView = this.audioFileViewStorage.GetById(imageView.FileId);

            return fileView?.Content;
        }

        public List<InterviewBinaryDataDescriptor> GetBinaryFilesForInterview(Guid interviewId)
        {
            throw new NotImplementedException();
        }

        public void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data)
        {
            var imageView =
                this.audioFileMetadataViewStorage.Where(image => image.InterviewId == interviewId && image.FileName == fileName)
                    .SingleOrDefault();

            if (imageView == null)
            {
                string fileId = Guid.NewGuid().FormatGuid();
                this.audioFileViewStorage.Store(new AudioFileView
                {
                    Id = fileId,
                    Content = data
                });

                this.audioFileMetadataViewStorage.Store(new AudioFileMetadataView
                {
                    Id = Guid.NewGuid().FormatGuid(),
                    InterviewId = interviewId,
                    FileId = fileId,
                    FileName = fileName
                });
            }
            else
            {
                this.audioFileViewStorage.Store(new AudioFileView
                {
                    Id = imageView.FileId,
                    Content = data
                });
            }
        }

        public void RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var imageView = this.audioFileMetadataViewStorage.Where(image => image.InterviewId == interviewId && image.FileName == fileName).SingleOrDefault();

            if (imageView == null) return;

            this.audioFileViewStorage.Remove(imageView.FileId);
            this.audioFileMetadataViewStorage.Remove(imageView.Id);
        }
    }
}