using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class StartInterviewViewModel : MvxNotifyPropertyChanged, ICompositeEntity
    {
        readonly IViewModelNavigationService viewModelNavigationService;
        public event EventHandler InterviewStarted;

        public StartInterviewViewModel(IViewModelNavigationService viewModelNavigationService)
        {
            this.viewModelNavigationService = viewModelNavigationService;
        }

        protected string interviewId;
        protected SourceScreen sourceScreen;

        public void Init(string interviewId, SourceScreen sourceScreen)
        {
            this.interviewId = interviewId;
            this.sourceScreen = sourceScreen;
        }

        public IMvxAsyncCommand StartInterviewCommand => new MvxAsyncCommand(this.StartInterviewAsync);

        private async Task StartInterviewAsync()
        {
            if (this.viewModelNavigationService.HasPendingOperations)
                this.viewModelNavigationService.ShowWaitMessage();
            else
            {
                await this.viewModelNavigationService.NavigateToInterviewAsync(interviewId, navigationIdentity: null,
                    sourceScreen: sourceScreen);
                this.InterviewStarted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
