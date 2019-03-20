using System.Collections.Generic;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class OptionWithSearchTerm
    {
        public int Value { get; set; }
        public string Title { get; set; }
        public string SearchTerm { get; set; }

        public override string ToString() => this.Title;
    }
}
