using System;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class BreadCrumbItemViewModel : MvxNotifyPropertyChanged,
        IDisposable
    {
        public DynamicTextViewModel Text { get; }

        public BreadCrumbItemViewModel(DynamicTextViewModel dynamicTextViewModel)
        {
            this.Text = dynamicTextViewModel;
        }

        private NavigationState navigationState;

        public Identity ItemId { get; private set; }

        public void Init(string interviewId, Identity itemIdentity, string text, NavigationState navigationState)
        {
            this.ItemId = itemIdentity;
            this.Text.Init(interviewId, itemIdentity, text);
            this.navigationState = navigationState;
        }

        public IMvxCommand NavigateCommand => new MvxCommand(() => this.navigationState.NavigateTo(NavigationIdentity.CreateForGroup(this.ItemId)));

        public void ChangeText(string newText) => this.Text.ChangeText(newText);

        public void Dispose()
        {
            this.Text.Dispose();
        }
    }
}