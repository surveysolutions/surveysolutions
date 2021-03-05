using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services;
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
        private readonly IEnumeratorEventStorage eventStorage;

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
            IInterviewerSettings interviewerSettings,
            IEnumeratorEventStorage eventStorage,
            IPrincipal principal) : base(interviewFactory, interviewMultimediaViewStorage, logger, imagesStorage, audioFileStorage, synchronizationService, audioAuditFileStorage, interviewViewRepository, principal, sortOrder)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.interviewerSettings = interviewerSettings;
            this.eventStorage = eventStorage;
        }

        protected override bool IsCompressEnabled()
        {
            return !interviewerSettings.PartialSynchronizationEnabled;
        }

        protected override bool IsNonPartialSyncedInterview(InterviewView interview)
        {
            return interview.Status == InterviewStatus.Completed 
                || interview.Status == InterviewStatus.WebInterview;
        }

        protected override IReadOnlyCollection<InterviewView> GetInterviewsForUpload()
        {
            if (interviewerSettings.PartialSynchronizationEnabled && interviewerSettings.AllowSyncWithHq)
            {
                return interviewViewRepository.Where(interview =>
                    interview.Status == InterviewStatus.Completed
                    || interview.Status == InterviewStatus.Restarted
                    || interview.Status == InterviewStatus.InterviewerAssigned
                    || interview.Status == InterviewStatus.RejectedBySupervisor
                ).Where(interview =>
                    interview.Status == InterviewStatus.Completed
                    || eventStorage.HasEventsWithoutHqFlag(interview.InterviewId)
                ).ToReadOnlyCollection();
            }

            return interviewViewRepository.Where(interview =>
                interview.Status == InterviewStatus.Completed ||
                interview.Status == InterviewStatus.WebInterview
            );
        }
    }
}
