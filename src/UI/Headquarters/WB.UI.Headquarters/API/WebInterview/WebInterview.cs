using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.API.WebInterview
{
    [HubName(@"interview")]
    public partial class WebInterview : Hub, IErrorDetailsProvider
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly ICommandService commandService;
        private readonly IMapper autoMapper;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IWebInterviewNotificationService webInterviewNotificationService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IChangeStatusFactory changeStatusFactory;
        private readonly IInterviewFactory interviewFactory;
        private readonly IStatefullInterviewSearcher statefullInterviewSearcher;

        private string CallerInterviewId => this.Context.QueryString[@"interviewId"];
        private string CallerSectionid => this.Clients.Caller.sectionId;

        private bool IsReviewMode => 
            this.authorizedUser.CanConductInterviewReview() &&
            this.Context.QueryString[@"review"].ToBool(false);

        private IStatefulInterview GetCallerInterview() => this.statefulInterviewRepository.Get(this.CallerInterviewId);

        private IQuestionnaire GetCallerQuestionnaire()
        {
            var interview = this.GetCallerInterview();
            return this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
        }

        public WebInterview(
            IStatefulInterviewRepository statefulInterviewRepository,
            ICommandService commandService,
            IMapper autoMapper,
            IQuestionnaireStorage questionnaireRepository,
            IWebInterviewNotificationService webInterviewNotificationService,
            IAuthorizedUser authorizedUser,
            IChangeStatusFactory changeStatusFactory,
            IInterviewFactory interviewFactory,
            IStatefullInterviewSearcher statefullInterviewSearcher)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.commandService = commandService;
            this.autoMapper = autoMapper;
            this.questionnaireRepository = questionnaireRepository;
            this.webInterviewNotificationService = webInterviewNotificationService;
            this.authorizedUser = authorizedUser;
            this.changeStatusFactory = changeStatusFactory;
            this.interviewFactory = interviewFactory;
            this.statefullInterviewSearcher = statefullInterviewSearcher;
        }


        public void FillExceptionData(Dictionary<string, string> data)
        {
            var interviewId = CallerInterviewId;
            if (interviewId != null) data["caller.interviewId"] = interviewId;
        }

        [Localizable(false)]
        public static string GetConnectedClientSectionKey(string sectionId, string interviewId) => $"{sectionId}x{interviewId}";

        [Localizable(false)]
        public static string GetConnectedClientPrefilledSectionKey(string interviewId) => $"PrefilledSectionx{interviewId}";
        
        public static string GetUiMessageFromException(Exception e)
        {
            if (e is InterviewException interviewException && interviewException.ExceptionType != InterviewDomainExceptionType.Undefined)
            {
                switch (interviewException.ExceptionType)
                {
                    case InterviewDomainExceptionType.InterviewLimitReached:
                        return Headquarters.Resources.WebInterview.ServerUnderLoad;
                    case InterviewDomainExceptionType.QuestionnaireIsMissing:
                    case InterviewDomainExceptionType.InterviewHardDeleted:
                        return Headquarters.Resources.WebInterview.Error_InterviewExpired;
                    case InterviewDomainExceptionType.OtherUserIsResponsible:
                    case InterviewDomainExceptionType.StatusIsNotOneOfExpected:
                        return Headquarters.Resources.WebInterview.Error_NoActionsNeeded;
                    case InterviewDomainExceptionType.InterviewRecievedByDevice:
                        return Headquarters.Resources.WebInterview.InterviewReceivedByInterviewer;
                }
            }

            return e.Message;
        }
    }
}