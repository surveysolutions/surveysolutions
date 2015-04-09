using System;
using Cirrious.MvvmCross.ViewModels;

using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class PrefilledQuestionsViewModel : BaseViewModel
    {
        private readonly ICommandService commandService;

        public PrefilledQuestionsViewModel(ILogger logger/*, ICommandService commandService*/)
            : base(logger)
        {
            //this.commandService = commandService;
        }

        private bool isInProgress = false;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set { isInProgress = value; RaisePropertyChanged(() => IsInProgress); }
        }

        public IMvxCommand OpenQuestionnaireCommand
        {
            get { return new MvxCommand(this.OpenQuestionnaire, () => !IsInProgress); }
        }

        private async void OpenQuestionnaire()
        {
            IsInProgress = true;

            try
            {
                this.ShowViewModel<DashboardViewModel>();
            }
            catch (Exception ex)
            {
               throw;
            }
            finally
            {
                IsInProgress = false;
            }
        }

        public override void NavigateToPreviousViewModel()
        {
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}