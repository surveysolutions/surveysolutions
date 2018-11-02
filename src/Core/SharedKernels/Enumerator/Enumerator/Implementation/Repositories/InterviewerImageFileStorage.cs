using System;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class InterviewerImageFileStorage : InterviewerFileStorage<InterviewMultimediaView, InterviewFileView>, IImageFileStorage
    {
        public InterviewerImageFileStorage(
            IPlainStorage<InterviewMultimediaView> imageViewStorage,
            IPlainStorage<InterviewFileView> fileViewStorage,
            IEncryptionService encryptionService)
            : base(imageViewStorage, fileViewStorage, encryptionService)
        {
        }

        public string GetPath(Guid interviewId, string filename = null)
        {
            return null;
        }
    }
}
