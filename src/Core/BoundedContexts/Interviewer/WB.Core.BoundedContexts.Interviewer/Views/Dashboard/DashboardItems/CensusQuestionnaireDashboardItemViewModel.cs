using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class CensusQuestionnaireDashboardItemViewModel : IDashboardItem
    {
        private readonly ICommandService commandService;
        private readonly IInterviewerPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxMessenger messenger;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;

        public CensusQuestionnaireDashboardItemViewModel(
            ICommandService commandService,
            IInterviewerPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IMvxMessenger messenger,
            IPlainStorage<InterviewView> interviewViewRepository)
        {
            this.commandService = commandService;
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
            this.messenger = messenger;
            this.interviewViewRepository = interviewViewRepository;
        }

        private QuestionnaireIdentity questionnaireIdentity;

        public void Init(QuestionnaireView questionnaire)
        {
            this.questionnaireIdentity = questionnaire.Identity;
            this.QuestionnaireName = string.Format(InterviewerUIResources.DashboardItem_Title, questionnaire.Title, questionnaire.Identity.Version);

            var interviewsByQuestionnareCount = this.interviewViewRepository.Count(interview => interview.QuestionnaireId == questionnaire.Id);

            this.Comment = InterviewerUIResources.DashboardItem_CensusModeComment.FormatString(interviewsByQuestionnareCount);
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
                interviewerIdentity.UserId, this.questionnaireIdentity, DateTime.UtcNow,
                interviewerIdentity.SupervisorId);
            await this.commandService.ExecuteAsync(createInterviewOnClientCommand);
            this.viewModelNavigationService.NavigateToPrefilledQuestions(interviewId.FormatGuid());
        }

        private void RaiseStartingLongOperation()
        {
            messenger.Publish(new StartingLongOperationMessage(this));
        }
    }
}