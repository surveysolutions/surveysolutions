using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var imageView = this.imageViewStorage.Query(
                images => images.FirstOrDefault(image => image.InterviewId == interviewId && image.FileName == fileName));

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
            string FileId = Guid.NewGuid().FormatGuid();

            new Task(async () =>
            {
                await this.fileViewStorage.StoreAsync(new InterviewFileView
                {
                    Id = FileId,
                    File = data
                }).ContinueWith(async x =>
                    await this.imageViewStorage.StoreAsync(new InterviewMultimediaView
                    {
                        Id = Guid.NewGuid().FormatGuid(),
                        InterviewId = interviewId,
                        FileId = FileId,
                        FileName = fileName
                    }));
            }).Start();
        }

        public void RemoveInterviewBinaryData(Guid interviewId, string fileName)
        {
            var imageView = this.imageViewStorage.Query(
                images => images.FirstOrDefault(image => image.InterviewId == interviewId && image.FileName == fileName));

            if (imageView == null) return;

            this.fileViewStorage.RemoveAsync(imageView.FileId).Wait();
            this.imageViewStorage.RemoveAsync(imageView.Id).Wait();
        }

        public void RemoveAllBinaryDataForInterview(Guid interviewId)
        {
            throw new NotImplementedException();
        }
    }
}