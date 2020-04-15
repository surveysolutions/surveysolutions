using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Interviewer.Services;
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
        private readonly IInterviewerSettings interviewerSettings;

        public InterviewerUploadInterviews(
            IInterviewerInterviewAccessor interviewFactory, 
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage, 
            ILogger logger, 
            IImageFileStorage imagesStorage, 
            IAudioFileStorage audioFileStorage, 
            ISynchronizationService synchronizationService, 
            IAudioAuditFileStorage audioAuditFileStorage,
            int sortOrder, 
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewerSettings interviewerSettings) : base(interviewFactory, interviewMultimediaViewStorage, logger, imagesStorage, audioFileStorage, synchronizationService, audioAuditFileStorage, interviewViewRepository, sortOrder)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.interviewerSettings = interviewerSettings;
        }

        protected override IReadOnlyCollection<InterviewView> GetInterviewsForUpload()
        {
            if (interviewerSettings.CustomSynchronizationEnabled && !interviewerSettings.AllowSyncWithHq)
            {
                return interviewViewRepository.Where(interview =>
                    interview.Status == InterviewStatus.Completed
                    || interview.Status == InterviewStatus.Restarted
                    || interview.Status == InterviewStatus.InterviewerAssigned
                );
            }

            return interviewViewRepository.Where(interview =>
                interview.Status == InterviewStatus.Completed
            );
        }
    }
}
