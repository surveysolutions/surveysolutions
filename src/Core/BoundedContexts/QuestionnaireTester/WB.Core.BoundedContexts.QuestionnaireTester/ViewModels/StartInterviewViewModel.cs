using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class StartInterviewViewModel : MvxViewModel,
        IInterviewEntityViewModel
    {
        private readonly ICommandService commandService;

        public StartInterviewViewModel(ICommandService commandService)
        {
            this.commandService = commandService;
        }

        private string interviewId;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
        }

        private IMvxCommand startInterviewCommand;
        public IMvxCommand StartInterviewCommand
        {
            get
            {
                return startInterviewCommand ?? (startInterviewCommand = new MvxCommand(async () => await this.StartInterview()));
            }
        }

        private async Task StartInterview()
        {
            await this.commandService.WaitPendingCommandsAsync();

            this.ShowViewModel<InterviewViewModel>(new { interviewId = this.interviewId });
        }
    }
}