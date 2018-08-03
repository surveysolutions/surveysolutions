using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public abstract class ListEntityViewModel : MvxViewModel
    {
        public abstract void Init(NavigationIdentity entityIdentity, string entityTitle, NavigationState navigationState);
    }

    public class EntityWithErrorsViewModel : ListEntityViewModel
    {
        public override void Init(NavigationIdentity entityIdentity, string entityTitle, NavigationState navigationState)
        {
            this.navigationState = navigationState;
            this.entityIdentity = entityIdentity;
            this.entityTitle = entityTitle;
        }

        private NavigationState navigationState;

        private string entityTitle;
        public string EntityTitle => this.entityTitle;

        private NavigationIdentity entityIdentity;

        public IMvxCommand NavigateToSectionCommand => new MvxAsyncCommand(async ()=> await this.navigationState.NavigateTo(this.entityIdentity));
    }

    public class EntityWithCommentsViewModel : EntityWithErrorsViewModel
    {
    }
}
