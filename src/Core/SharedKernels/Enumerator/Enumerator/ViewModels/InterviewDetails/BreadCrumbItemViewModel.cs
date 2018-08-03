using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
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

        public void Init(string interviewId, Identity itemIdentity, NavigationState navigationState)
        {
            this.ItemId = itemIdentity;
            this.Text.Init(interviewId, itemIdentity);
            this.navigationState = navigationState;
        }

        public IMvxCommand NavigateCommand => new MvxAsyncCommand(async () => await this.navigationState.NavigateTo(NavigationIdentity.CreateForGroup(this.ItemId)));

        public void Dispose()
        {
            this.Text.Dispose();
        }
    }
}
