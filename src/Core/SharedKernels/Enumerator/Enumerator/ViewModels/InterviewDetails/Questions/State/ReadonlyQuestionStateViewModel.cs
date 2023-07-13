using System;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class ReadonlyQuestionStateViewModel: MvxNotifyPropertyChanged,
        IQuestionStateViewModel
    {
        public QuestionHeaderViewModel Header { get; private set; }
        public virtual ValidityViewModel Validity { get; private set; }
        public virtual WarningsViewModel Warnings { get; }
        public EnablementViewModel Enablement { get; private set; }
        public CommentsViewModel Comments { get; private set; }

        protected ReadonlyQuestionStateViewModel() { }

        public ReadonlyQuestionStateViewModel(
            ValidityViewModel validityViewModel, 
            QuestionHeaderViewModel questionHeaderViewModel,
            EnablementViewModel enablementViewModel,
            CommentsViewModel commentsViewModel,
            WarningsViewModel warningsViewModel)
        {
            this.Warnings = warningsViewModel;
            this.Validity = validityViewModel;
            this.Header = questionHeaderViewModel;
            this.Enablement = enablementViewModel;
            this.Comments = commentsViewModel;
        }
        
        public virtual void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.Validity.Init(interviewId, entityIdentity, navigationState);
            this.Warnings.Init(interviewId, entityIdentity, navigationState);
            this.Comments.Init(interviewId, entityIdentity, navigationState);
            this.Enablement.Init(interviewId, entityIdentity);
            this.Header.Init(interviewId, entityIdentity, this.Enablement, navigationState);

            this.Header.ShowComments += this.ShowCommentsCommand;
        }

        public IMvxCommand ShowCommentEditorCommand =>  new MvxCommand(() => this.ShowCommentsCommand(this, EventArgs.Empty));

        private void ShowCommentsCommand(object sender, EventArgs eventArgs)
        {
            if (this.Enablement.Enabled)
            {
                this.Comments.ShowCommentInEditor();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Header?.Dispose();
                Validity?.Dispose();
                Warnings?.Dispose();
                Enablement?.Dispose();
                Comments?.Dispose();
            }
        }
    }
}
