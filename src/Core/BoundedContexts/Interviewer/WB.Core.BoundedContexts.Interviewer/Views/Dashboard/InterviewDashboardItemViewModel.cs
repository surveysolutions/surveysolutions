using System;
using System.Collections.Generic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class InterviewDashboardItemViewModel : IDashboardItem
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IAnswerToStringService answerToStringService;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IUserInteractionService userInteractionService;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IChangeLogManipulator changeLogManipulator;

        public class PrefilledQuestion
        {
            public string Question { get; set; }
            public string Answer { get; set; }
        }

        public string QuestionariName { get; private set; }

        public Guid InterviewId { get; private set; }

        public DashboardInterviewCategories Status { get; private set; }

        public DateTime? StartedDate { get; private set; }
        public DateTime? ComplitedDate { get; private set; }
        public DateTime? RejectedDate { get; private set; }

        public List<PrefilledQuestion> PrefilledQuestions { get; private set; }


        public InterviewDashboardItemViewModel(IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IAnswerToStringService answerToStringService,
            IViewModelNavigationService viewModelNavigationService,
            IUserInteractionService userInteractionService,
            ICommandService commandService,
            IPrincipal principal,
            IChangeLogManipulator changeLogManipulator)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.answerToStringService = answerToStringService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.userInteractionService = userInteractionService;
            this.commandService = commandService;
            this.principal = principal;
            this.changeLogManipulator = changeLogManipulator;
        }

        public void Init(IStatefulInterview interview, DashboardInterviewCategories interviewCategories)
        {
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            InterviewId = interview.Id;
            QuestionariName = questionnaire.Title;
            ComplitedDate = DateTime.Now;
            RejectedDate = DateTime.Now;
            StartedDate = DateTime.Now;
            Status = interviewCategories;

            PrefilledQuestions = new List<PrefilledQuestion>();
            var prefilledQuestionsIds = questionnaire.PrefilledQuestionsIds;
            foreach (var prefilledQuestionsId in prefilledQuestionsIds)
            {
                var baseQuestionModel = questionnaire.Questions[prefilledQuestionsId.Id];
                var identityAsString = ConversionHelper.ConvertIdAndRosterVectorToString(prefilledQuestionsId.Id, new decimal[0]);

                if (!interview.Answers.ContainsKey(identityAsString))
                    continue;

                var baseInterviewAnswer = interview.Answers[identityAsString];

                var prefilledQuestion = new PrefilledQuestion();
                prefilledQuestion.Question = baseQuestionModel.Title;
                prefilledQuestion.Answer = answerToStringService.AnswerToUIString(baseQuestionModel,
                    baseInterviewAnswer, interview, questionnaire);
                PrefilledQuestions.Add(prefilledQuestion);
            }
        }

        public IMvxCommand LoadDashboardItemCommand
        {
            get { return new MvxCommand(LoadInterview); }
        }

        private async void LoadInterview()
        {
            if (Status == DashboardInterviewCategories.Complited)
            {
                var isReopen = await userInteractionService.ConfirmAsync(
                    InterviewerUIResources.Dashboard_Reinitialize_Interview_Message,
                    okButton: UIResources.Yes,
                    cancelButton: UIResources.No);

                if (!isReopen)
                {
                    return;
                }

                var restartInterviewCommand = new RestartInterviewCommand(InterviewId, principal.CurrentUserIdentity.UserId, "", DateTime.UtcNow);
                await commandService.ExecuteAsync(restartInterviewCommand);
                changeLogManipulator.CreateOrReopenDraftRecord(InterviewId, principal.CurrentUserIdentity.UserId);
            }

            viewModelNavigationService.NavigateToInterview(InterviewId.FormatGuid());
        }
    }
}