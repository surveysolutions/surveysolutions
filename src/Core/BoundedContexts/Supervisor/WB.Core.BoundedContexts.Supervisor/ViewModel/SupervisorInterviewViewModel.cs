using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class SupervisorInterviewViewModel : BaseInterviewViewModel
    {
        public SupervisorInterviewViewModel(IQuestionnaireStorage questionnaireRepository, 
            IStatefulInterviewRepository interviewRepository, SideBarSectionsViewModel sectionsViewModel,
            BreadCrumbsViewModel breadCrumbsViewModel, NavigationState navigationState, 
            AnswerNotifier answerNotifier, GroupStateViewModel groupState, InterviewStateViewModel interviewState, 
            CoverStateViewModel coverState, IPrincipal principal, IViewModelNavigationService viewModelNavigationService, 
            IInterviewViewModelFactory interviewViewModelFactory, ICommandService commandService, 
            VibrationViewModel vibrationViewModel, IEnumeratorSettings enumeratorSettings) 
            : base(questionnaireRepository, interviewRepository, sectionsViewModel, breadCrumbsViewModel, navigationState, 
                answerNotifier, groupState, interviewState, coverState, principal, viewModelNavigationService, 
                interviewViewModelFactory, commandService, vibrationViewModel, enumeratorSettings)
        {
        }

        public override IMvxCommand ReloadCommand => new MvxAsyncCommand(async () 
            => await this.ViewModelNavigationService.NavigateToInterviewAsync(this.InterviewId, this.NavigationState.CurrentNavigationIdentity));


        public override async Task NavigateBack()
        {
            await this.ViewModelNavigationService.NavigateToDashboardAsync(this.InterviewId);
            this.Dispose();
        }

        protected override MvxViewModel UpdateCurrentScreenViewModel(ScreenChangedEventArgs eventArgs)
        {
            switch (this.NavigationState.CurrentScreenType)
            {
                case ScreenType.Complete:
                    var completeInterviewViewModel = this.interviewViewModelFactory.GetNew<SupervisorResolveInterviewViewModel>();
                    completeInterviewViewModel.Configure(this.InterviewId, this.NavigationState);
                    return completeInterviewViewModel;
                default:
                    return base.UpdateCurrentScreenViewModel(eventArgs);
            }
        }
    }
}
