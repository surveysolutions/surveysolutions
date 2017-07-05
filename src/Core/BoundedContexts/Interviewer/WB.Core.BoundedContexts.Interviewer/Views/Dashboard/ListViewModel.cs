using System.Collections.Generic;
using MvvmCross.Core.ViewModels;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public abstract class ListViewModel<TListItem> : InterviewTabPanel where TListItem: class
    {
        private MvxObservableCollection<TListItem> uiItems = new MvxObservableCollection<TListItem>();
        public MvxObservableCollection<TListItem> UiItems {
            get => this.uiItems;
            protected set => this.RaiseAndSetIfChanged(ref this.uiItems, value);
        }

        public abstract int ItemsCount { get; }
    }
}