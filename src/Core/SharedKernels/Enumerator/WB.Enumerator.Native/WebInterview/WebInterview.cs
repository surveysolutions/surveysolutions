﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNet.SignalR;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Enumerator.Native.WebInterview
{
    public partial class WebInterview : Hub, IErrorDetailsProvider
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        protected readonly ICommandService commandService;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IWebInterviewNotificationService webInterviewNotificationService;
        private readonly IWebInterviewInterviewEntityFactory interviewEntityFactory;

        private string CallerInterviewId => this.Context.QueryString[@"interviewId"];
        private string CallerSectionid => this.Clients.Caller.sectionId;

        protected IStatefulInterview GetCallerInterview() => this.statefulInterviewRepository.Get(this.CallerInterviewId);

        private IQuestionnaire GetCallerQuestionnaire()
        {
            var interview = this.GetCallerInterview();
            return this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
        }

        protected virtual bool IsReviewMode => false;

        protected virtual bool IsCurrentUserObserving => false;

        public WebInterview(
            IStatefulInterviewRepository statefulInterviewRepository,
            ICommandService commandService,
            IQuestionnaireStorage questionnaireRepository,
            IWebInterviewNotificationService webInterviewNotificationService,
            IWebInterviewInterviewEntityFactory interviewEntityFactory)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.commandService = commandService;
            this.questionnaireRepository = questionnaireRepository;
            this.webInterviewNotificationService = webInterviewNotificationService;
            this.interviewEntityFactory = interviewEntityFactory;
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