using System.Collections.Generic;
using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class NewInterviewsViewModel : ListViewModel<IDashboardItem>
    {
        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private SynchronizationViewModel synchronizationViewModel;

        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPrincipal principal;
        public int NewInterviewsCount => this.Items.OfType<InterviewDashboardItemViewModel>().Count();
        public int CensusQuestionnairesCount => this.Items.Count() - this.NewInterviewsCount;
        public override GroupStatus InterviewStatus => GroupStatus.Disabled;
        
        public IMvxCommand SynchronizationCommand => new MvxCommand(this.RunSynchronization);

        private bool hasInterviews;
        public bool HasInterviews
        {
            get { return this.hasInterviews; }
            set { this.hasInterviews = value; this.RaisePropertyChanged(); }
        }

        private void RunSynchronization()
        {
            this.synchronizationViewModel.IsSynchronizationInProgress = true;
            this.synchronizationViewModel.Synchronize();
        }

        public NewInterviewsViewModel(
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IPrincipal principal)
        {
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.interviewViewRepository = interviewViewRepository;
            this.viewModelFactory = viewModelFactory;
            this.principal = principal;
        }

        public void Load(SynchronizationViewModel sync)
        {
            this.synchronizationViewModel = sync;

            var newInterviews = this.GetNewInterviews().ToList<IDashboardItem>();
            this.HasInterviews = newInterviews.Count > 0;
            if (this.HasInterviews)
            {
                this.Items = newInterviews;
                this.Title = string.Format(InterviewerUIResources.Dashboard_NewItemsLinkText, newInterviews.Count);
            }
            else
            {
                this.Items = this.GetCensusQuestionnaires().ToList<IDashboardItem>();
                this.Title = InterviewerUIResources.Dashboard_NoNewItemsLinkText;
            }
            
        }

        private List<CensusQuestionnaireDashboardItemViewModel> GetCensusQuestionnaires()
        {
            var censusQuestionnireViews = this.questionnaireViewRepository.Where(questionnaire => questionnaire.Census);

            // show census mode for new tab
            var censusQuestionnaireViewModels = new List<CensusQuestionnaireDashboardItemViewModel>();
            foreach (var censusQuestionnireView in censusQuestionnireViews)
            {
                var censusQuestionnaireDashboardItem = this.viewModelFactory.GetNew<CensusQuestionnaireDashboardItemViewModel>();
                censusQuestionnaireDashboardItem.Init(censusQuestionnireView);
                censusQuestionnaireViewModels.Add(censusQuestionnaireDashboardItem);
            }

            return censusQuestionnaireViewModels;
        }

        private IEnumerable<InterviewDashboardItemViewModel> GetNewInterviews()
        {
            var interviewerId = this.principal.CurrentUserIdentity.UserId;
            var interviewViews = this.interviewViewRepository.Where(interview =>
                interview.ResponsibleId == interviewerId &&
                interview.Status == SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus.InterviewerAssigned &&
                interview.StartedDateTime == null);

            foreach (var interviewView in interviewViews)
            {
                var interviewDashboardItem = this.viewModelFactory.GetNew<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(interviewView);

                yield return interviewDashboardItem;
            }
        }
    }
}