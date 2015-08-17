﻿using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class StartInterviewViewModel : MvxNotifyPropertyChanged
    {
        private readonly ICommandService commandService;
        readonly IViewModelNavigationService viewModelNavigationService;

        public StartInterviewViewModel(ICommandService commandService, IViewModelNavigationService viewModelNavigationService)
        {
            this.commandService = commandService;
            this.viewModelNavigationService = viewModelNavigationService;
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

            this.viewModelNavigationService.NavigateToInterview(interviewId);
        }
    }
}