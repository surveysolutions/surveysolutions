using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNet.SignalR;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Enumerator.Native.WebInterview
{
    public abstract partial class WebInterview : Hub, IErrorDetailsProvider
    {
        //private readonly IStatefulInterviewRepository statefulInterviewRepository;
        //protected readonly ICommandService commandService;
        //private readonly IQuestionnaireStorage questionnaireRepository;
        //private readonly IWebInterviewNotificationService webInterviewNotificationService;
        //private readonly IWebInterviewInterviewEntityFactory interviewEntityFactory;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAudioFileStorage audioFileStorage;

        protected string CallerInterviewId => this.Context.QueryString[@"interviewId"];
        private string CallerSectionid => this.Clients.Caller.sectionId;

        protected IStatefulInterview GetCallerInterview(IStatefulInterviewRepository statefulInterviewRepository)
        {
            return statefulInterviewRepository.Get(this.CallerInterviewId);
        }

        protected IStatefulInterview GetCallerInterview()
        {
            IStatefulInterview interview = null;
            InScopeExecutor.Current.ExecuteActionInScope(sl =>
            {
                interview = GetCallerInterview(sl.GetInstance<IStatefulInterviewRepository>());
            });
            return interview;
        }

        protected IQuestionnaire GetCallerQuestionnaire()
        {
            IQuestionnaire questionnaire = null;
            InScopeExecutor.Current.ExecuteActionInScope(sl =>
            {
                var interview = GetCallerInterview(sl.GetInstance<IStatefulInterviewRepository>());
                if (interview == null)
                    return;

                questionnaire = sl.GetInstance<IQuestionnaireStorage>().GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            });

            return questionnaire;
        }

        protected IQuestionnaire GetCallerQuestionnaire(IServiceLocator sl)
        {
            IQuestionnaire questionnaire = null;
            
            var interview = GetCallerInterview(sl.GetInstance<IStatefulInterviewRepository>());
            if (interview == null)
                return null;

            questionnaire = sl.GetInstance<IQuestionnaireStorage>().GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            

            return questionnaire;
        }

        protected virtual bool IsReviewMode => false;

        protected virtual bool IsCurrentUserObserving => false;

        public WebInterview(
            //IStatefulInterviewRepository statefulInterviewRepository,
            //ICommandService commandService,
            //IQuestionnaireStorage questionnaireRepository,
            //IWebInterviewNotificationService webInterviewNotificationService,
            //IWebInterviewInterviewEntityFactory interviewEntityFactory,
            IImageFileStorage imageFileStorage,
            IAudioFileStorage audioFileStorage)
        {
            //this.statefulInterviewRepository = statefulInterviewRepository ?? throw new ArgumentNullException(nameof(statefulInterviewRepository));
            //this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            //this.questionnaireRepository = questionnaireRepository ?? throw new ArgumentNullException(nameof(questionnaireRepository));
            //this.webInterviewNotificationService = webInterviewNotificationService ?? throw new ArgumentNullException(nameof(webInterviewNotificationService));
            //this.interviewEntityFactory = interviewEntityFactory ?? throw new ArgumentNullException(nameof(interviewEntityFactory));
            this.audioFileStorage = audioFileStorage ?? throw new ArgumentNullException(nameof(audioFileStorage));
            this.imageFileStorage = imageFileStorage ?? throw new ArgumentNullException(nameof(imageFileStorage));
        }

        public void FillExceptionData(Dictionary<string, string> data)
        {
            var interviewId = CallerInterviewId;
            if (interviewId != null) data["caller.interviewId"] = interviewId;
        }

        [Localizable(false)]
        public static string GetConnectedClientSectionKey(Identity sectionId, Guid interviewId) => $"{sectionId}x{interviewId}";

        [Localizable(false)]
        public static string GetConnectedClientPrefilledSectionKey(Guid interviewId) => $"PrefilledSectionx{interviewId}";
        
        public static string GetUiMessageFromException(Exception e)
        {
            if (e is InterviewException interviewException && interviewException.ExceptionType != InterviewDomainExceptionType.Undefined)
            {
                switch (interviewException.ExceptionType)
                {
                    case InterviewDomainExceptionType.InterviewLimitReached:
                        return Enumerator.Native.Resources.WebInterview.ServerUnderLoad;
                    case InterviewDomainExceptionType.QuestionnaireIsMissing:
                    case InterviewDomainExceptionType.InterviewHardDeleted:
                        return Enumerator.Native.Resources.WebInterview.Error_InterviewExpired;
                    case InterviewDomainExceptionType.OtherUserIsResponsible:
                    case InterviewDomainExceptionType.StatusIsNotOneOfExpected:
                        return Enumerator.Native.Resources.WebInterview.Error_NoActionsNeeded;
                    case InterviewDomainExceptionType.InterviewRecievedByDevice:
                        return Enumerator.Native.Resources.WebInterview.InterviewReceivedByInterviewer;
                }
            }

            return e.Message;
        }
    }
}
