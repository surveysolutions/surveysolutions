using System.Collections.Generic;
using MvvmCross.Core.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class ExpandableQuestionsDashboardItemViewModel : MvxNotifyPropertyChanged, IDashboardItem
    {

        public bool HasExpandedView { get; protected set; }

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => this.isExpanded;
            set
            {
                if (this.isExpanded == value) return;

                this.isExpanded = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => this.PrefilledQuestions);
            }
        }

        public List<PrefilledQuestion> PrefilledQuestions  =>
            this.IsExpanded ? this.DetailedIdentifyingData : this.IdentifyingData;
        
        protected List<PrefilledQuestion> IdentifyingData;
        protected List<PrefilledQuestion> DetailedIdentifyingData;
    }
}