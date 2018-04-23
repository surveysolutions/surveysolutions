using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class StartInterviewViewModel : MvxNotifyPropertyChanged, ICompositeEntity
    {
        private readonly ICommandService commandService;
        readonly IViewModelNavigationService viewModelNavigationService;
        public event EventHandler InterviewStarted;

        public StartInterviewViewModel(ICommandService commandService, IViewModelNavigationService viewModelNavigationService)
        {
            this.commandService = commandService;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        protected string interviewId;

        public void Init(string interviewId)
        {
            this.interviewId = interviewId;
        }

        private IMvxCommand startInterviewCommand;
        public IMvxCommand StartInterviewCommand
        {
            get
            {
                return this.startInterviewCommand ?? (this.startInterviewCommand = new MvxAsyncCommand(async () => await this.StartInterviewAsync()));
            }
        }

        private async Task StartInterviewAsync()
        {
            await this.commandService.WaitPendingCommandsAsync();
            await this.viewModelNavigationService.NavigateToInterviewAsync(interviewId, navigationIdentity: null);
            this.OnInterviewStarted();
        }

        protected virtual void OnInterviewStarted()
        {
            this.InterviewStarted?.Invoke(this, EventArgs.Empty);
        }
    }
}
