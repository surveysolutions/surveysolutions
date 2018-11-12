using System;
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class StaticTextViewModel : 
        IInterviewEntityViewModel,
        IDisposable
    {
        private NavigationState navigationState;
        public DynamicTextViewModel Text { get; }
        public AttachmentViewModel Attachment { get; }
        public StaticTextStateViewModel QuestionState { get; }

        public StaticTextViewModel(
            DynamicTextViewModel dynamicTextViewModel,
            AttachmentViewModel attachmentViewModel,
            StaticTextStateViewModel questionState)
        {
            this.Text = dynamicTextViewModel;
            this.Attachment = attachmentViewModel;
            this.QuestionState = questionState;
        }

        public Identity Identity { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.navigationState = navigationState;
            this.Identity = entityIdentity;

            this.Text.Init(interviewId, entityIdentity);
            this.Attachment.Init(interviewId, entityIdentity);
            this.QuestionState.Init(interviewId, entityIdentity);
        }

        public IMvxAsyncCommand ShowPdf =>
            new MvxAsyncCommand(() => this.navigationState.NavigateTo(NavigationIdentity.CreateForPdfView(this.Identity)));

        public void Dispose()
        {
            this.QuestionState.Dispose();
            this.Text.Dispose();
        }
    }
}
