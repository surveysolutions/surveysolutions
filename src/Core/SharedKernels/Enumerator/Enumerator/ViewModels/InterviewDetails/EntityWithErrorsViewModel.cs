using MvvmCross.Core.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public abstract class ListEntityViewModel : MvxViewModel
    {
        public abstract void Init(NavigationIdentity entityIdentity, string errorMessage, NavigationState navigationState);
    }

    public class EntityWithErrorsViewModel : ListEntityViewModel
    {
        public override void Init(NavigationIdentity entityIdentity, string errorMessage, NavigationState navigationState)
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

    public class EntityWithCommentsViewModel : EntityWithErrorsViewModel
    {
    }
}