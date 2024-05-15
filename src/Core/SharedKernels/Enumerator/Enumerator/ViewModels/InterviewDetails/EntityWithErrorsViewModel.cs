using System.Collections.Generic;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public abstract class ListEntityViewModel : BaseViewModel
    {
        public abstract void Init(NavigationIdentity entityIdentity, string title, NavigationState navigationState);
    }

    public class EntityWithErrorsViewModel : ListEntityViewModel, IInterviewEntity
    {
        public override void Init(NavigationIdentity entityIdentity, string title, NavigationState navigationState)
        {
            this.NavigationState = navigationState;
            this.entityIdentity = entityIdentity;
            this.entityTitle = title;
            this.IsError = true;
        }

        private string entityTitle;
        private NavigationIdentity entityIdentity;
        public string EntityTitle => this.entityTitle;
        
        public bool IsError { get; set; }

        public IMvxCommand NavigateToSectionCommand => 
            new MvxAsyncCommand(async ()=> await this.NavigationState.NavigateTo(this.entityIdentity), () => HasNavigation);

        public bool HasNavigation => entityIdentity != null;
        
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

        public string InterviewId { get; set; }
        public Identity Identity { get; set; }
        public NavigationState NavigationState { get; set; }

        public void AllowInnerLinks(string intrviewId, Identity identity)
        {
            this.InterviewId = intrviewId;
            this.Identity = identity;
        }
    }

    public class EntityWithCommentsViewModel : EntityWithErrorsViewModel
    {
    }    
    
    public class CompleteGroup : MvxObservableCollection<EntityWithErrorsViewModel>
    {
        public CompleteGroup()
        {
        }

        public CompleteGroup(IEnumerable<EntityWithErrorsViewModel> items) : base(items)
        {
        }

        public int AllCount { get; set; }
        public CompleteGroupContent GroupContent { get; set; }

        public bool HasChildren => AllCount > 0;
        public bool Expanded => false;
        public string Title { get; set; }
    }
    
    public enum CompleteGroupContent
    {
        Unknown,
        Error,
        Answered,
        Unanswered,
    }

}
