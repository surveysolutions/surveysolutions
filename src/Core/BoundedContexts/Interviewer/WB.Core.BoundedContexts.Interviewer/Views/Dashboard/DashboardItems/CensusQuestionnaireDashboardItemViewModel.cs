using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Interviewer.ViewModel.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class CensusQuestionnaireDashboardItemViewModel : IDashboardItem
    {
        private readonly ICommandService commandService;
        private readonly IChangeLogManipulator changeLogManipulator;
        private readonly IPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;

        public CensusQuestionnaireDashboardItemViewModel(
            ICommandService commandService,
            IChangeLogManipulator changeLogManipulator,
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.commandService = commandService;
            this.changeLogManipulator = changeLogManipulator;
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        private Guid questionnaireId;
        private long questionnaireVersion;

        public void Init(SurveyDto surveyDto, int countInterviewsFromCurrentQuestionnare)
        {
            this.questionnaireId = Guid.Parse(surveyDto.QuestionnaireId);
            this.questionnaireVersion = surveyDto.QuestionnaireVersion;
            this.QuestionariName = string.Format(InterviewerUIResources.DashboardItem_Title, surveyDto.SurveyTitle, surveyDto.QuestionnaireVersion);
            this.Comment = InterviewerUIResources.DashboardItem_CensusModeComment.FormatString(countInterviewsFromCurrentQuestionnare);
        }

        public string QuestionariName { get; set; }
        public string Comment { get; set; }



        public IMvxCommand CreateNewInterviewCommand
        {
            get { return new MvxCommand(this.CreateNewInterview); }
        }

        private async void CreateNewInterview()
        {
            var interviewId = Guid.NewGuid();
            var interviewerIdentity = (InterviewerIdentity)this.principal.CurrentUserIdentity;

            var createInterviewOnClientCommand = new CreateInterviewOnClientCommand(interviewId, interviewerIdentity.UserId, this.questionnaireId, this.questionnaireVersion, DateTime.UtcNow, interviewerIdentity.SupervisorId);
            await this.commandService.ExecuteAsync(createInterviewOnClientCommand);
            this.changeLogManipulator.CreatePublicRecord(interviewId);
            this.viewModelNavigationService.NavigateToPrefilledQuestions(interviewId.FormatGuid());
        }
    }
}