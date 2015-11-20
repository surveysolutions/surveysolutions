using System;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Interviewer.ViewModel.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class CensusQuestionnaireDashboardItemViewModel : IDashboardItem
    {
        private readonly ICommandService commandService;
        private readonly IChangeLogManipulator changeLogManipulator;
        private readonly IInterviewerPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxMessenger messenger;

        public CensusQuestionnaireDashboardItemViewModel(
            ICommandService commandService,
            IChangeLogManipulator changeLogManipulator,
            IInterviewerPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IMvxMessenger messenger)
        {
            this.commandService = commandService;
            this.changeLogManipulator = changeLogManipulator;
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
            this.messenger = messenger;
        }

        private Guid questionnaireId;
        private long questionnaireVersion;

        public void Init(SurveyDto surveyDto, int countInterviewsFromCurrentQuestionnare)
        {
            this.questionnaireId = Guid.Parse(surveyDto.QuestionnaireId);
            this.questionnaireVersion = surveyDto.QuestionnaireVersion;
            this.QuestionnaireName = string.Format(InterviewerUIResources.DashboardItem_Title, surveyDto.SurveyTitle, surveyDto.QuestionnaireVersion);
            this.Comment = InterviewerUIResources.DashboardItem_CensusModeComment.FormatString(countInterviewsFromCurrentQuestionnare);
        }

        public string QuestionnaireName { get; set; }
        public string Comment { get; set; }


        public IMvxCommand CreateNewInterviewCommand
        {
            get { return new MvxCommand(async () => await this.CreateNewInterviewAsync()); }
        }

        private async Task CreateNewInterviewAsync()
        {
            RaiseStartingLongOperation();
            var interviewId = Guid.NewGuid();
            var interviewerIdentity = this.principal.CurrentUserIdentity;

            var createInterviewOnClientCommand = new CreateInterviewOnClientCommand(interviewId,
                interviewerIdentity.UserId, this.questionnaireId, this.questionnaireVersion, DateTime.UtcNow,
                interviewerIdentity.SupervisorId);
            await this.commandService.ExecuteAsync(createInterviewOnClientCommand);

            await Task.Run(async () =>
            {
                this.changeLogManipulator.CreatePublicRecord(interviewId);
                await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(interviewId.FormatGuid());
            });
        }

        private void RaiseStartingLongOperation()
        {
            messenger.Publish(new StartingLongOperationMessage(this));
        }
    }
}