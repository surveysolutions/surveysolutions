using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class StaticTextViewModel : 
        IInterviewEntityViewModel,
        IInterviewEntity,
        IDisposable
    {
        public DynamicTextViewModel Text { get; }
        public AttachmentViewModel Attachment { get; }
        public StaticTextStateViewModel StaticTextState { get; }

        public StaticTextViewModel(
            DynamicTextViewModel dynamicTextViewModel,
            AttachmentViewModel attachmentViewModel,
            StaticTextStateViewModel questionState)
        {
            this.Text = dynamicTextViewModel;
            this.Attachment = attachmentViewModel;
            this.StaticTextState = questionState;
        }

        public string InterviewId { get; private set; }
        public Identity Identity { get; private set; }
        public NavigationState NavigationState { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.InterviewId = interviewId;
            this.NavigationState = navigationState;
            this.Identity = entityIdentity;

            this.Text.Init(interviewId, entityIdentity);
            this.Attachment.Init(interviewId, entityIdentity, navigationState);
            this.StaticTextState.Init(interviewId, entityIdentity, navigationState);
        }

        public void Dispose()
        {
            this.Attachment.ViewDestroy();
            this.StaticTextState.Dispose();
            this.Text.Dispose();
        }
    }
}
