using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public abstract class BaseInterviewsViewModel : ListViewModel
    {
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;

        protected BaseInterviewsViewModel(IInterviewViewModelFactory viewModelFactory, 
            IPlainStorage<InterviewView> interviewViewRepository)
        {
            this.viewModelFactory = viewModelFactory;
            this.interviewViewRepository = interviewViewRepository;
        }
        
        private int? highLightedItemIndex;

        protected abstract string TabTitle { get; }
        protected abstract string TabDescription { get; }
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

        public async Task Load()
        {
            this.ItemsCount = this.GetDbItemsCount();
            this.UpdateTitle();

            var uiItems = await Task.Run(() => this.GetUiItems());
            this.UiItems.ReplaceWith(uiItems);
        }

        protected virtual IEnumerable<IDashboardItem> GetUiItems()
        {
            var subTitle = this.viewModelFactory.GetNew<DashboardSubTitleViewModel>();
            subTitle.Title = this.TabDescription;

            yield return subTitle;
            
            foreach (var interviewView in this.GetDbItems())
            {
                var interviewDashboardItem = this.viewModelFactory.GetNew<InterviewDashboardItemViewModel>();
                interviewDashboardItem.Init(interviewView);

                this.OnItemCreated(interviewDashboardItem);

                yield return interviewDashboardItem;
            }
        }

        public void HighLight(InterviewDashboardItemViewModel dashboardItem)
        {
            var index = this.UiItems.IndexOf(dashboardItem);
            if (index > 0)
            {
                this.HighLightedItemIndex = index;
            }
        }
    }
}