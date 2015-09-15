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
using WB.UI.Interviewer.ViewModel.Dashboard;

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



        public string QuestionariName { get; private set; }
        public Guid InterviewId { get; private set; }
        public DashboardInterviewCategories Status { get; private set; }
        public List<PrefilledQuestion> PrefilledQuestions { get; private set; }
        public string DateComment { get; private set; }
        public string Comment { get; private set; }

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

        public void Init(DashboardQuestionnaireItem item, DashboardInterviewCategories interviewCategories)
        {
            //var questionnaire = this.questionnaireRepository.GetById(item.QuestionnaireId);

            InterviewId = item.SurveyKey;
            Status = interviewCategories;
            QuestionariName = string.Format("{0} (v{1})", item.Title, item.QuestionnaireVersion);
//            DateComment = GetInterviewDateCommentByStatus(item, Status);
//            Comment = GetInterviewCommentByStatus(item, Status);
//            PrefilledQuestions = this.GetPrefilledQuestions(item, questionnaire);
            IsSupportedRemove = item.CanBeDeleted;
        }

        private string GetInterviewDateCommentByStatus(IStatefulInterview interview, DashboardInterviewCategories status)
        {
            switch (status)
            {
                case DashboardInterviewCategories.New:
                    return "Created on {0}".FormatString(interview.CreatedDateTime);
                case DashboardInterviewCategories.InProgress:
                    return "Started on {0}".FormatString(interview.StartedDateTime);
                case DashboardInterviewCategories.Complited:
                case DashboardInterviewCategories.Rejected:
                    return "Complited on {0}".FormatString(interview.ComplitedDateTime);
                default:
                    return string.Empty;
            }
        }

        private string GetInterviewCommentByStatus(IStatefulInterview interview, DashboardInterviewCategories status)
        {
            switch (status)
            {
                case DashboardInterviewCategories.New:
                    return "Not started";
                case DashboardInterviewCategories.InProgress:
                    var answeredCount = interview.CountAnsweredQuestionsInInterview();
                    return "Answered: {0}, Unanswered: {1}, Errors:{2}".FormatString(
                        answeredCount,
                        interview.CountActiveQuestionsInInterview() - answeredCount,
                        interview.CountInvalidQuestionsInInterview());
                case DashboardInterviewCategories.Complited:
                    return interview.InterviewerCompliteComment;
                case DashboardInterviewCategories.Rejected:
                    return interview.SupervisorRejectComment;
                default:
                    return string.Empty;
            }
        }

        private List<PrefilledQuestion> GetPrefilledQuestions(IStatefulInterview interview, QuestionnaireModel questionnaire)
        {
            var listQuestions = new List<PrefilledQuestion>();

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
                prefilledQuestion.Answer = this.answerToStringService.AnswerToUIString(baseQuestionModel,
                    baseInterviewAnswer, interview, questionnaire);
                listQuestions.Add(prefilledQuestion);
            }

            return listQuestions;
        }

        public bool IsSupportedRemove { get; set; }

        public IMvxCommand RemoveInterviewCommand
        {
            get { return new MvxCommand(RemoveInterview); }
        }

        private async void RemoveInterview()
        {
            var isNeedDelete = await userInteractionService.ConfirmAsync(
                InterviewerUIResources.Dashboard_RemoveInterviewQuestion.FormatString(QuestionariName),
                okButton: UIResources.Yes,
                cancelButton: UIResources.No);

            if (!isNeedDelete)
                return;

            var deleteInterviewCommand = new DeleteInterviewCommand(InterviewId, principal.CurrentUserIdentity.UserId);
            await commandService.ExecuteAsync(deleteInterviewCommand);
            changeLogManipulator.CleanUpChangeLogByEventSourceId(InterviewId);
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