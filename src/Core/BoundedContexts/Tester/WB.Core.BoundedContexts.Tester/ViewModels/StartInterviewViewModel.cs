using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.InterviewEntities;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.Questions;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class StartInterviewViewModel : MvxViewModel,
        IInterviewEntityViewModel
    {
        private readonly ICommandService commandService;
        private readonly IViewModelNavigationService viewModelNavigationService;

        public StartInterviewViewModel(ICommandService commandService, IViewModelNavigationService viewModelNavigationService)
        {
            this.commandService = commandService;
            this.viewModelNavigationService = viewModelNavigationService;
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

            this.viewModelNavigationService.NavigateTo<InterviewViewModel>(new { interviewId = this.interviewId });
        }
    }
}