using System.Collections.Generic;
using MvvmCross.Core.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class ExpandableQuestionsDashboardItemViewModel : MvxNotifyPropertyChanged, IDashboardItem
    {
        public bool HasExpandedView { get; set; }
        private bool isExpanded = false;

        public bool IsExpanded
        {
            get => this.isExpanded;
            set => this.RaiseAndSetIfChanged(ref this.isExpanded, value, alsoNotify: nameof(PrefilledQuestions));
        }

        public List<PrefilledQuestion> PrefilledQuestions =>
            this.IsExpanded ? this.DetailedIdentifyingData : this.IdentifyingData;
        
        protected List<PrefilledQuestion> IdentifyingData;
        protected List<PrefilledQuestion> DetailedIdentifyingData;
    }
}