using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Synchronization
{
    public class InterviewerUploadInterviews : UploadInterviews
    {
        private readonly IPlainStorage<InterviewView> interviewViewRepository;

        public InterviewerUploadInterviews(IInterviewerInterviewAccessor interviewFactory, 
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage, 
            ILogger logger, 
            IPlainStorage<InterviewFileView> imagesStorage, 
            IAudioFileStorage audioFileStorage, 
            ISynchronizationService synchronizationService, 
            int sortOrder, 
            IPlainStorage<InterviewView> interviewViewRepository) : base(interviewFactory, interviewMultimediaViewStorage, logger, imagesStorage, audioFileStorage, synchronizationService, sortOrder)
        {
            this.interviewViewRepository = interviewViewRepository;
        }

        protected override IReadOnlyCollection<InterviewView> GetInterviewsForUpload()
        {
            return interviewViewRepository.Where(interview => interview.Status == InterviewStatus.Completed);
        }
    }
}
