using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

public class QuestionAttachmentViewModel : MvxNotifyPropertyChanged, ICompositeEntity
{
    public AttachmentViewModel Attachment { get; }

    public QuestionAttachmentViewModel(AttachmentViewModel attachmentViewModel)
    {
        Attachment = attachmentViewModel;
    }
}
