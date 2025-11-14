using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class TabletImageFileStorage : InterviewFileStorage<InterviewMultimediaView, InterviewFileView>, IImageFileStorage
    {
        public TabletImageFileStorage(
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

        public Task RemoveAllBinaryDataForInterviewsAsync(List<Guid> interviewIds)
        {
            throw new NotImplementedException();
        }
    }
}
