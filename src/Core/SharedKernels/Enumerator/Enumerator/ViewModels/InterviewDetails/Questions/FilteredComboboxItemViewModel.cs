namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class FilteredComboboxItemViewModel
    {
        public string Text { get; set; }
        public int Value { get; set; }

        public override string ToString()
        {
            return this.Text.Replace("</b>", "").Replace("<b>", "");
        }
    }

    public class CascadingComboboxItemViewModel : FilteredComboboxItemViewModel
    {
        public string OriginalText { get; set; }
        public int ParentValue { get; set; }
    }
}