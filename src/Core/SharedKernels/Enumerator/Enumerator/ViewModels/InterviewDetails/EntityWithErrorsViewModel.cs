using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public abstract class ListEntityViewModel : BaseViewModel
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
            this.IsError = true;
        }

        private NavigationState navigationState;

        private string entityTitle;
        public string EntityTitle => this.entityTitle;
        
        public bool IsError { get; private set; }

        private NavigationIdentity entityIdentity;

        public IMvxCommand NavigateToSectionCommand => 
            new MvxAsyncCommand(async ()=> await this.navigationState.NavigateTo(this.entityIdentity), () => entityIdentity != null);

        public static EntityWithErrorsViewModel InitTitle(string title)
        {
            var viewModel = new EntityWithErrorsViewModel();
            viewModel.Init(null, title, null);
            viewModel.IsError = false;
            return viewModel;
        }

        public static EntityWithErrorsViewModel InitError(string title)
        {
            var viewModel = new EntityWithErrorsViewModel();
            viewModel.Init(null, title, null);
            return viewModel;
        }
    }

    public class EntityWithCommentsViewModel : EntityWithErrorsViewModel
    {
    }    
    
    public class FailCriticalityConditionViewModel : BaseViewModel
    {
        public string EntityTitle { get; private set; }

        public void Init(string title)
        {
            this.EntityTitle = title;
        }
    }
}
