using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using System;
using MvvmCross.Platform;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable.Services;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public interface IAssignmentItemService
    {
        Task CreateInterviewAsync(AssignmentDocument assignment);
        int GetInterviewsCount(int assignmentId);
    }

    public class CreateNewViewModel : ListViewModel<IDashboardItem>, IAssignmentItemService
    {
        public override int ItemsCount => this.UiItems.Count(
            item => item is CensusQuestionnaireDashboardItemViewModel || item is AssignmentDashboardItemViewModel);

        public override GroupStatus InterviewStatus => GroupStatus.Disabled;

        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IAssignmentDocumentsStorage assignmentsRepository;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IInterviewerPrincipal interviewerPrincipal;
        private readonly IMvxMessenger messenger;
        private readonly ICommandService commandService;
        private readonly IInterviewAnswerSerializer answerSerializer;
        private SynchronizationViewModel Synchronization;

        private IMvxCommand synchronizationCommand;
        private DashboardViewModel dashboardViewModel;

        public IMvxCommand SynchronizationCommand => synchronizationCommand ??
                                                     (synchronizationCommand = new MvxCommand(this.RunSynchronization, 
                                                         () => !this.Synchronization.IsSynchronizationInProgress));

        public CreateNewViewModel(
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IAssignmentDocumentsStorage assignmentsRepository,
            IPlainStorage<InterviewView> interviewViewRepository,
            IViewModelNavigationService viewModelNavigationService,
            IInterviewerPrincipal interviewerPrincipal,
            IMvxMessenger messenger,
            ICommandService commandService,
            IInterviewAnswerSerializer answerSerializer)
        {
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.viewModelFactory = viewModelFactory;
            this.assignmentsRepository = assignmentsRepository;
            this.interviewViewRepository = interviewViewRepository;
            this.viewModelNavigationService = viewModelNavigationService;
            this.interviewerPrincipal = interviewerPrincipal;
            this.messenger = messenger;
            this.commandService = commandService;
            this.answerSerializer = answerSerializer;
        }

        public async Task LoadAsync(SynchronizationViewModel sync, DashboardViewModel dashboardViewModel)
        {
            this.Synchronization = sync;
            this.Title = InterviewerUIResources.Dashboard_AssignmentsTabTitle;
            this.dashboardViewModel = dashboardViewModel;

            dashboardViewModel.IsInProgress = true;
            try
            {
                var assignments = await Task.Run(() => this.GetCensusQuestionnaires().Union(this.GetAssignments(dashboardViewModel)).ToList());

                var uiItems = assignments.Any()
                    ? new List<IDashboardItem>(assignments.Count + 1) { this.GetSubTitle() }
                    : new List<IDashboardItem>(assignments.Count);

                uiItems.AddRange(assignments);
                UiItems.ReplaceWith(uiItems);
            }
            finally
            {
                dashboardViewModel.IsInProgress = false;
            }
        }

        private void RunSynchronization()
        {
            if (this.viewModelNavigationService.HasPendingOperations)
            {
                this.viewModelNavigationService.ShowWaitMessage();
                return;
            }

            this.Synchronization.IsSynchronizationInProgress = true;
            this.Synchronization.Synchronize();
        }

        private IDashboardItem GetSubTitle()
        {
            var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
            subTitle.Title = InterviewerUIResources.Dashboard_CreateNewTabText;
            return subTitle;
        }

        private IEnumerable<IDashboardItem> GetCensusQuestionnaires()
        {
            foreach (var censusQuestionnaireView in this.questionnaireViewRepository.Where(questionnaire => questionnaire.Census))
            {
                var censusQuestionnaireDashboardItem = this.viewModelFactory.GetNew<CensusQuestionnaireDashboardItemViewModel>();
                censusQuestionnaireDashboardItem.Init(censusQuestionnaireView);

                yield return censusQuestionnaireDashboardItem;
            }
        }

        private IEnumerable<IDashboardItem> GetAssignments(DashboardViewModel dashboardViewModel)
        {
            var interviewsCount = this.interviewViewRepository.LoadAll().ToLookup(iv => iv.Assignment);

            foreach (var assignment in this.assignmentsRepository.LoadAll())
            {
                var dashboardItem = this.viewModelFactory.GetNew<AssignmentDashboardItemViewModel>();
                dashboardItem.Init(assignment, dashboardViewModel, this, interviewsCount[assignment.Id].Count());

                yield return dashboardItem;
            }
        }

        public int GetInterviewsCount(int assignmentId)
        {
            return this.interviewViewRepository.Count(iv => iv.Assignment == assignmentId);
        }

        public async Task CreateInterviewAsync(AssignmentDocument assignment)
        {
            try
            {
                this.dashboardViewModel.IsInProgress = true;

                var interviewId = Guid.NewGuid();
                var interviewerIdentity = this.interviewerPrincipal.CurrentUserIdentity;
                this.assignmentsRepository.FetchPreloadedData(assignment);
                var questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);

                List<InterviewAnswer> answers = this.GetAnswers(assignment.Answers);

                ICommand createInterviewCommand = new CreateInterviewOnClientCommand(interviewId,
                    interviewerIdentity.UserId,
                    new QuestionnaireIdentity(questionnaireIdentity.QuestionnaireId,
                        questionnaireIdentity.Version),
                    DateTime.UtcNow,
                    interviewerIdentity.SupervisorId,
                    null,
                    assignment.Id,
                    answers
                );

                await this.commandService.ExecuteAsync(createInterviewCommand);
                this.viewModelNavigationService.NavigateToPrefilledQuestions(interviewId.FormatGuid());
            }
            catch (InterviewException e)
            {
                // This code is going to be removed after KP-9461. And according to research in KP-9513 we should reduce amount of dependencies in constructor

                var userInteractionService = Mvx.Resolve<IUserInteractionService>();
                Mvx.Resolve<ILoggerProvider>().GetFor<AssignmentDashboardItemViewModel>().Error(e.Message, e);
                await userInteractionService.AlertAsync(string.Format(InterviewerUIResources.FailedToCreateInterview, e.Message), UIResources.Error);
            }
            finally
            {
                this.dashboardViewModel.IsInProgress = false;
            }
        }

        private List<InterviewAnswer> GetAnswers(List<AssignmentDocument.AssignmentAnswer> identifyingAnswers)
        {
            var elements = identifyingAnswers
                .Select(ia => new InterviewAnswer
                {
                    Identity = ia.Identity,
                    Answer = this.answerSerializer.Deserialize<AbstractAnswer>(ia.SerializedAnswer)
                })
                .Where(x => x.Answer != null)
                .ToList();

            return elements;
        }
    }
}