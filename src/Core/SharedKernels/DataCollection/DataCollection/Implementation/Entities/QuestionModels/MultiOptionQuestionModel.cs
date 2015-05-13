using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels
{
    public class MultiOptionQuestionModel : BaseQuestionModel
    {
        public bool AreAnswersOrdered { get; set; }

        public int? MaxAllowedAnswers { get; set; }

        public List<OptionModel> Options { get; set; }

        public bool IsRosterSizeQuestion { get; set; }
    }
}