using System.Collections.Generic;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class EntityWithErrorsViewModel : MvxViewModel, IInterviewEntity
    {
        public void Init(NavigationIdentity entityIdentity, 
            string title, 
            string comment, 
            string error, 
            NavigationState navigationState)
        {
            this.NavigationState = navigationState;
            this.entityIdentity = entityIdentity;
            this.entityTitle = title;
            this.Comment = title;
            this.Error = title;
            this.Comment = comment;
            this.Error = error;
            this.IsError = true;
        }

        public string Comment { get; private set; }
        public string Error { get; private set; }

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
            viewModel.Init(null, title, null, null, null);
            viewModel.IsError = false;
            return viewModel;
        }

        public static EntityWithErrorsViewModel InitError(string title)
        {
            var viewModel = new EntityWithErrorsViewModel();
            viewModel.Init(null, null, null, title, null);
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
    
    
    
    
    public enum CompleteGroupContent
    {
        Unknown,
        Error,
        Answered,
        Unanswered,
    }
}
