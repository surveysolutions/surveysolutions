using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard
{
    public abstract class BaseInterviewsViewModel : ListViewModel
    {
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo;

        protected BaseInterviewsViewModel(IInterviewViewModelFactory viewModelFactory, 
            IPlainStorage<InterviewView> interviewViewRepository,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo)
        {
            this.viewModelFactory = viewModelFactory;
            this.interviewViewRepository = interviewViewRepository;
            this.identifyingQuestionsRepo = identifyingQuestionsRepo;
        }
        
        private int? highLightedItemIndex;
        private Guid? lastVisitedInterviewId;

        public abstract string TabTitle { get; }
        public abstract string TabDescription { get; }
        protected abstract Expression<Func<InterviewView, bool>> GetDbQuery();
        protected virtual void OnItemCreated(InterviewDashboardItemViewModel interviewDashboardItem) { }

        protected void UpdateTitle() => this.Title = string.Format(this.TabTitle, this.ItemsCount);

        public int? HighLightedItemIndex
        {
            get => highLightedItemIndex;
            set => SetProperty(ref highLightedItemIndex, value);
        }

        private IReadOnlyCollection<InterviewView> GetDbItems()
            => this.interviewViewRepository.Where(this.GetDbQuery());

        private int GetDbItemsCount()
            => this.interviewViewRepository.Count(this.GetDbQuery());

        public void Load(Guid? lastVisitedInterviewId)
        {
            this.lastVisitedInterviewId = lastVisitedInterviewId;

            this.ItemsCount = this.GetDbItemsCount();
            this.UpdateTitle();

            this.UpdateUiItems();
        }

        protected override IEnumerable<IDashboardItem> GetUiItems()
        {
            var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
            subTitle.Title = this.TabDescription;

            yield return subTitle;

            var interviewIndex = 1;

            var preffilledQuestions = this.identifyingQuestionsRepo
                .LoadAll().ToLookup(d => d.InterviewId);
            
            foreach (var interviewView in this.GetDbItems())
            {
                var interviewDashboardItem = this.viewModelFactory.GetNew<InterviewDashboardItemViewModel>();

                var details = preffilledQuestions[interviewView.InterviewId]
                    .OrderBy(x => x.SortIndex)
                    .Select(fi => new PrefilledQuestion {Answer = fi.Answer?.Trim(), Question = fi.QuestionText})
                    .ToList();

                interviewDashboardItem.Init(interviewView, details);

                this.OnItemCreated(interviewDashboardItem);

                if (interviewDashboardItem.InterviewId == lastVisitedInterviewId)
                    this.HighLightedItemIndex = interviewIndex;

                interviewIndex++;

                yield return interviewDashboardItem;
            }
        }
    }
}
