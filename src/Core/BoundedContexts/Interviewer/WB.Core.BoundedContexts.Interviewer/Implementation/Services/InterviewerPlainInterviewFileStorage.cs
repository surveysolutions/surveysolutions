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
    public class InterviewerPlainInterviewFileStorage : IPlainInterviewFileStorage
    {
        private readonly IAsyncPlainStorage<InterviewMultimediaView> imageViewStorage;
        private readonly IAsyncPlainStorage<InterviewFileView> fileViewStorage;

        public InterviewerPlainInterviewFileStorage(
            IAsyncPlainStorage<InterviewMultimediaView> imageViewStorage,
            IAsyncPlainStorage<InterviewFileView> fileViewStorage)
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

            return fileView?.File;
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
                this.fileViewStorage.Store(new InterviewFileView
                {
                    Id = fileId,
                    File = data
                });

                this.imageViewStorage.Store(new InterviewMultimediaView
                {
                    Id = Guid.NewGuid().FormatGuid(),
                    InterviewId = interviewId,
                    FileId = fileId,
                    FileName = fileName
                });
            }
            else
            {
                this.fileViewStorage.Store(new InterviewFileView
                {
                    Id = imageView.FileId,
                    File = data
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