using System;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerImageFileStorage : InterviewerFileStorage<InterviewMultimediaView, InterviewFileView>, IImageFileStorage
    {
        public InterviewerImageFileStorage(
            IPlainStorage<InterviewMultimediaView> imageViewStorage,
            IPlainStorage<InterviewFileView> fileViewStorage)
            : base(imageViewStorage, fileViewStorage)
        {
        }
    }
}