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

            this.Identity = entityIdentity;

            this.Text.Init(interviewId, entityIdentity);
            this.Attachment.Init(interviewId, entityIdentity, navigationState);
            this.QuestionState.Init(interviewId, entityIdentity);
        }

        public void Dispose()
        {
            this.QuestionState.Dispose();
            this.Text.Dispose();
        }
    }
}
