using MvvmCross.Core.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class EntityWithErrorsViewModel : MvxViewModel
    {
        public void Init(NavigationIdentity entityIdentity, string errorMessage, NavigationState navigationState)
        {
            this.navigationState = navigationState;
            this.entityIdentity = entityIdentity;
            this.entityTitle = errorMessage;
        }

        private NavigationState navigationState;

        private string entityTitle;
        public string EntityTitle => this.entityTitle;

        private NavigationIdentity entityIdentity;

        public IMvxCommand NavigateToSectionCommand => new MvxCommand(()=> this.navigationState.NavigateTo(this.entityIdentity));
    }
}