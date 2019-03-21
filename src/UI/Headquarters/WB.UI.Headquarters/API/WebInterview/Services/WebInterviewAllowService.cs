using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    class WebInterviewAllowService : IWebInterviewAllowService
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IAuthorizedUser authorizedUser;
        private readonly EventBusSettings eventBusSettings;

        private static readonly List<InterviewStatus> AllowedInterviewStatuses = new List<InterviewStatus>
        {
            InterviewStatus.SupervisorAssigned,
            InterviewStatus.InterviewerAssigned,
            InterviewStatus.Restarted,
            InterviewStatus.RejectedBySupervisor
        };

        private static readonly List<InterviewStatus> AnonymousUserAllowedStatuses = new List<InterviewStatus>
        {
            InterviewStatus.SupervisorAssigned,
            InterviewStatus.InterviewerAssigned
        };

        public WebInterviewAllowService(
            IStatefulInterviewRepository statefulInterviewRepository,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IAuthorizedUser authorizedUser,
            EventBusSettings EventBusSettings)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.authorizedUser = authorizedUser;
            this.eventBusSettings = EventBusSettings;
        }

        public void CheckWebInterviewAccessPermissions(string interviewId)
        {
            if(this.eventBusSettings.IgnoredAggregateRoots.Contains(interviewId))
                if (!this.authorizedUser.IsHeadquarter && !this.authorizedUser.IsAdministrator)
                {
                    throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound, Enumerator.Native.Resources.WebInterview.Error_NotFound);
                }
                else
                {
                    return;
                }

            var interview = statefulInterviewRepository.Get(interviewId);

            if (interview == null)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound, Enumerator.Native.Resources.WebInterview.Error_NotFound);

            if (!AllowedInterviewStatuses.Contains(interview.Status))
                throw new InterviewAccessException(InterviewAccessExceptionReason.NoActionsNeeded, Enumerator.Native.Resources.WebInterview.Error_NoActionsNeeded);

            if (this.authorizedUser.IsInterviewer)
            {
                if (interview.CurrentResponsibleId == this.authorizedUser.Id)
                    return;
                else
                {
                    throw new InterviewAccessException(InterviewAccessExceptionReason.Forbidden,
                        Enumerator.Native.Resources.WebInterview.Error_Forbidden);
                }
            }

            QuestionnaireIdentity questionnaireIdentity = interview.QuestionnaireIdentity;

            WebInterviewConfig webInterviewConfig = webInterviewConfigProvider.Get( questionnaireIdentity);

            //interview is not public available and logged in user is not current interview responsible
            if (!webInterviewConfig.Started && interview.Status == InterviewStatus.InterviewerAssigned && this.authorizedUser.IsAuthenticated)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.UserNotAuthorised,
                    Enumerator.Native.Resources.WebInterview.Error_UserNotAuthorised);
            }

            if (!webInterviewConfig.Started || !AnonymousUserAllowedStatuses.Contains(interview.Status))
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }
        }
    }
}
