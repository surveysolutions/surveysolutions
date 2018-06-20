using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
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
