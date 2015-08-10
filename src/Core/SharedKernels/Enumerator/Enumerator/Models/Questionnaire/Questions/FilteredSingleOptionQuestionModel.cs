using System.Collections.Generic;

namespace WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions
{
    public class FilteredSingleOptionQuestionModel : BaseQuestionModel
    {
        public List<OptionModel> Options { get; set; }
    }
}