using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

public class CategoricalComboboxAutocompleteWithAttachmentViewModel : CategoricalComboboxAutocompleteViewModel
{
    public AttachmentViewModel Attachment { get; }

    public CategoricalComboboxAutocompleteWithAttachmentViewModel(IQuestionStateViewModel questionState, 
        FilteredOptionsViewModel filteredOptionsViewModel, bool displaySelectedValue,
        AttachmentViewModel attachment) 
        : base(questionState, filteredOptionsViewModel, displaySelectedValue)
    {
        Attachment = attachment;
    }

    public override void Dispose()
    {
        base.Dispose();
        
        Attachment?.ViewDestroy();
        Attachment?.Dispose();
    }
}
