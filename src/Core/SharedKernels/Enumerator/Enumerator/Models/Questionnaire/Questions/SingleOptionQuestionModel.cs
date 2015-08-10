using System.Collections.Generic;

namespace WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions
{
    public class SingleOptionQuestionModel : BaseQuestionModel
    {
        public List<OptionModel> Options { get; set; }
    }
}