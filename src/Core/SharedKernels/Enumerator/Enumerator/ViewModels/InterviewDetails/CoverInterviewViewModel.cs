using System;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CoverInterviewViewModel : MvxViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxMessenger messenger;
        private readonly ICommandService commandService;
        private readonly IEntityWithErrorsViewModelFactory entityWithErrorsViewModelFactory;
        protected readonly IPrincipal principal;

        public InterviewStateViewModel InterviewState { get; set; }
        public DynamicTextViewModel Name { get; }

        public CoverInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
            IPrincipal principal,
            IMvxMessenger messenger,
            IEntityWithErrorsViewModelFactory entityWithErrorsViewModelFactory,
            InterviewStateViewModel interviewState,
            DynamicTextViewModel dynamicTextViewModel)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.commandService = commandService;
            this.principal = principal;
            this.messenger = messenger;
            this.entityWithErrorsViewModelFactory = entityWithErrorsViewModelFactory;

            this.InterviewState = interviewState;
            this.Name = dynamicTextViewModel;
        }

        protected Guid interviewId;

        public virtual void Init(string interviewId, NavigationState navigationState)
        {
            this.interviewId = Guid.Parse(interviewId);

            this.InterviewState.Init(interviewId, null);
            this.Name.InitAsStatic(UIResources.Interview_Cover_Screen_Title);
        }
    }
}