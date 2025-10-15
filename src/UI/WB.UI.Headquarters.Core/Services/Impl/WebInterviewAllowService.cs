using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Code.WebInterview;

namespace WB.UI.Headquarters.Services.Impl
{
    class WebInterviewAllowService : IWebInterviewAllowService
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IAggregateRootPrototypeService prototypeService;
        private readonly IUserViewFactory usersRepository;
        private readonly IHttpContextAccessor contextAccessor;

        private static readonly List<InterviewStatus> AllowedInterviewStatuses = new()
        {
            InterviewStatus.SupervisorAssigned,
            InterviewStatus.InterviewerAssigned,
            InterviewStatus.Restarted,
            InterviewStatus.RejectedBySupervisor
        };

        public WebInterviewAllowService(
            IStatefulInterviewRepository statefulInterviewRepository,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IAuthorizedUser authorizedUser,
            IAggregateRootPrototypeService prototypeService,
            IUserViewFactory usersRepository,
            IHttpContextAccessor contextAccessor)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.authorizedUser = authorizedUser;
            this.prototypeService = prototypeService;
            this.usersRepository = usersRepository;
            this.contextAccessor = contextAccessor;
        }

        public void CheckWebInterviewAccessPermissions(string interviewId)
        {
            if (Guid.TryParse(interviewId, out var id))
            {
                if (this.prototypeService.IsPrototype(id))
                {
                    return;
                }
            }

            var interview = statefulInterviewRepository.Get(interviewId);

            if (interview == null)
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound, 
                    Enumerator.Native.Resources.WebInterview.Error_NotFound);

            //finish page for anonymous for completed interview
            if ( (!this.authorizedUser.IsAuthenticated || this.authorizedUser.IsInterviewer) 
                 && interview.Status == InterviewStatus.Completed)
            {
                var hasAccess = contextAccessor.HttpContext.Session.HasAccessToWebInterviewAfterComplete(interview);
                if (hasAccess)
                    return;
            }
            
            if (!AllowedInterviewStatuses.Contains(interview.Status))
                throw new InterviewAccessException(InterviewAccessExceptionReason.NoActionsNeeded, 
                    Enumerator.Native.Resources.WebInterview.Error_NoActionsNeeded);

            //is user in role of interviewer 
            var responsible = this.usersRepository.GetUser(interview.CurrentResponsibleId);
            if (responsible == null || !responsible.IsInterviewer())
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }

            if (this.authorizedUser.IsInterviewer)
            {
                if (interview.CurrentResponsibleId != this.authorizedUser.Id)
                {
                    throw new InterviewAccessException(InterviewAccessExceptionReason.Forbidden,
                        Enumerator.Native.Resources.WebInterview.Error_Forbidden);
                }
                else
                    return;
            }

            QuestionnaireIdentity questionnaireIdentity = interview.QuestionnaireIdentity;

            WebInterviewConfig webInterviewConfig = webInterviewConfigProvider.Get(questionnaireIdentity);

            //interview is not public available and logged in user is not current interview responsible
            if (!webInterviewConfig.Started && interview.Status == InterviewStatus.InterviewerAssigned
                && this.authorizedUser.IsAuthenticated)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.UserNotAuthorised,
                    Enumerator.Native.Resources.WebInterview.Error_UserNotAuthorised);
            }

            if (!webInterviewConfig.Started)
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }

            if (!interview.AcceptsCAWIAnswers())
            {
                throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewExpired,
                    Enumerator.Native.Resources.WebInterview.Error_InterviewExpired);
            }
        }
    }
}
