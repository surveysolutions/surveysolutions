using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public abstract class BaseInterviewsViewModel : ListViewModel
    {
        private readonly IInterviewViewModelFactory viewModelFactory;

        protected BaseInterviewsViewModel(IInterviewViewModelFactory viewModelFactory)
        {
            this.viewModelFactory = viewModelFactory;
        }

        private IReadOnlyCollection<InterviewView> dbItems;

        protected abstract string TabTitle { get; }
        protected abstract string TabDescription { get; }
        protected abstract IReadOnlyCollection<InterviewView> GetDbItems();
        protected virtual void OnItemCreated(InterviewDashboardItemViewModel interviewDashboardItem) { }

        protected void UpdateTitle() => this.Title = string.Format(this.TabTitle, this.ItemsCount);

        public async Task Load()
        {
            this.dbItems = this.GetDbItems();

            this.ItemsCount = this.dbItems.Count;
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
    }
}