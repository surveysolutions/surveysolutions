﻿using System.Collections.Generic;
using System.ComponentModel;
using AutoMapper;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.Headquarters.API.WebInterview
{
    [HubName(@"interview")]
    public partial class WebInterview : Hub, IErrorDetailsProvider
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly ICommandService commandService;
        private readonly IMapper autoMapper;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private IWebInterviewNotificationService webInterviewNotificationService;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;

        private string CallerInterviewId => this.Context.QueryString[@"interviewId"];
        private string CallerSectionid => this.Clients.Caller.sectionId;

        private IStatefulInterview GetCallerInterview() => this.statefulInterviewRepository.Get(this.CallerInterviewId);

        private IQuestionnaire GetCallerQuestionnaire()
            => this.questionnaireRepository.GetQuestionnaire(this.GetCallerInterview().QuestionnaireIdentity,
                this.GetCallerInterview().Language);

        public WebInterview(
            IStatefulInterviewRepository statefulInterviewRepository,
            ICommandService commandService,
            IMapper autoMapper,
            IQuestionnaireStorage questionnaireRepository,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IWebInterviewNotificationService webInterviewNotificationService,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.commandService = commandService;
            this.autoMapper = autoMapper;
            this.questionnaireRepository = questionnaireRepository;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.webInterviewNotificationService = webInterviewNotificationService;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
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
    }
}