using System;
using System.Linq;
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
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class CensusQuestionnaireDashboardItemViewModel : IDashboardItem
    {
        private readonly ICommandService commandService;
        private readonly IChangeLogManipulator changeLogManipulator;
        private readonly IInterviewerPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxMessenger messenger;
        private readonly IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IAsyncPlainStorage<InterviewView> interviewViewRepository;

        public CensusQuestionnaireDashboardItemViewModel(
            ICommandService commandService,
            IChangeLogManipulator changeLogManipulator,
            IInterviewerPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IMvxMessenger messenger,
            IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IAsyncPlainStorage<InterviewView> interviewViewRepository)
        {
            this.commandService = commandService;
            this.changeLogManipulator = changeLogManipulator;
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
            this.messenger = messenger;
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.interviewViewRepository = interviewViewRepository;
        }

        private QuestionnaireIdentity questionnaireIdentity;

        public void Init(string questionnaireId)
        {
            var questionnaire = this.questionnaireViewRepository.GetById(questionnaireId);

            this.questionnaireIdentity = questionnaire.Identity;
            this.QuestionnaireName = string.Format(InterviewerUIResources.DashboardItem_Title, questionnaire.Title, questionnaire.Identity.Version);

            var countInterviewsFromCurrentQuestionnare = this.interviewViewRepository.Query(
                interviews => interviews.Count(interview => interview.QuestionnaireIdentity.Equals(questionnaire.Identity)));

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
                interviewerIdentity.UserId, this.questionnaireIdentity.QuestionnaireId, this.questionnaireIdentity.Version, DateTime.UtcNow,
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