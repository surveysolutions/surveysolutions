using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.Messages;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class CensusQuestionnaireDashboardItemViewModel : MvxNotifyPropertyChanged, IDashboardItem, IDisposable
    {
        private readonly ICommandService commandService;
        private readonly IInterviewerPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxMessenger messenger;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IInterviewUniqueKeyGenerator keyGenerator;

        public CensusQuestionnaireDashboardItemViewModel(
            ICommandService commandService,
            IInterviewerPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IMvxMessenger messenger,
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewUniqueKeyGenerator keyGenerator)
        {
            this.commandService = commandService;
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
            this.messenger = messenger;
            this.interviewViewRepository = interviewViewRepository;
            this.keyGenerator = keyGenerator;
        }

        private QuestionnaireIdentity questionnaireIdentity;
        private string subTitle;

        public void Init(QuestionnaireView questionnaire)
        {
            this.questionnaireIdentity = QuestionnaireIdentity.Parse(questionnaire.Id);
            this.Title = string.Format(InterviewerUIResources.DashboardItem_Title, questionnaire.Title, this.questionnaireIdentity.Version);

            UpdateSubtitle();
        }

        public void UpdateSubtitle()
        {
            var questionnaireId = this.questionnaireIdentity.ToString();
            var interviewsByQuestionnareCount = this.interviewViewRepository.Count(interview => interview.QuestionnaireId == questionnaireId);
            this.SubTitle = InterviewerUIResources.DashboardItem_CensusModeComment.FormatString(interviewsByQuestionnareCount);
        }

        public DashboardInterviewStatus Status => DashboardInterviewStatus.Assignment;
        public string Title { get; set; }

        public string SubTitle
        {
            get => subTitle;
            set
            {
                if (value == subTitle) return;
                subTitle = value;
                RaisePropertyChanged(() => SubTitle);
            }
        }

        public IMvxCommand CreateNewInterviewCommand => new MvxAsyncCommand(this.CreateNewInterviewAsync);

        public bool HasExpandedView => false;
        
        public bool IsExpanded { get; set; }

        private async Task CreateNewInterviewAsync()
        {
            RaiseStartingLongOperation();
            var interviewId = Guid.NewGuid();
            var interviewerIdentity = this.principal.CurrentUserIdentity;

            var createInterviewCommand = new CreateInterview(interviewId,
                interviewerIdentity.UserId, this.questionnaireIdentity, 
                new List<InterviewAnswer>(),
                new List<string>(),
                interviewerIdentity.SupervisorId,
                interviewerIdentity.UserId,
                keyGenerator.Get(),
                null);
            await this.commandService.ExecuteAsync(createInterviewCommand);
            await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(interviewId.FormatGuid());
        }

        private void RaiseStartingLongOperation() => this.messenger.Publish(new StartingLongOperationMessage(this));

        public void Dispose()
        {
            interviewViewRepository?.Dispose();
        }
    }
}
