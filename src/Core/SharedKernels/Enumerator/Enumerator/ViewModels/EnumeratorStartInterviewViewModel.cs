using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class EnumeratorStartInterviewViewModel : MvxNotifyPropertyChanged
    {
        private readonly ICommandService commandService;

        protected EnumeratorStartInterviewViewModel(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        protected string interviewId;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
        }

        private IMvxCommand startInterviewCommand;
        public IMvxCommand StartInterviewCommand
        {
            get
            {
                return this.startInterviewCommand ?? (this.startInterviewCommand = new MvxCommand(async () => await this.StartInterviewAsync()));
            }
        }

        private async Task StartInterviewAsync()
        {
            await this.commandService.WaitPendingCommandsAsync();

            this.NavigateToInterview();
        }

        protected abstract void NavigateToInterview();
    }
}