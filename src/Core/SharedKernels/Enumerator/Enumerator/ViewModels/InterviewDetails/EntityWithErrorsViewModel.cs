using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class EntityWithErrorsViewModel : MvxViewModel
    {
        public async void Init(NavigationIdentity entityIdentity, string errorMessage, NavigationState navigationState)
        {
            this.navigationState = navigationState;
            this.entityIdentity = entityIdentity;
            this.entityTitle = errorMessage;
        }

        private NavigationState navigationState;

        private string entityTitle;
        public string EntityTitle => this.entityTitle;

        private NavigationIdentity entityIdentity;

        private MvxCommand navigateToSectionCommand;

        public ICommand NavigateToSectionCommand
        {
            get
            {
                this.navigateToSectionCommand = this.navigateToSectionCommand ?? new MvxCommand(async () => await this.NavigateAsync());
                return this.navigateToSectionCommand;
            }
        }

        private async Task NavigateAsync()
        {
            await this.navigationState.NavigateToAsync(entityIdentity);
        }
    }
}