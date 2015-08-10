using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions
{
    public class CascadingSingleOptionQuestionModel : BaseQuestionModel
    {
        public List<CascadingOptionModel> Options { get; set; }

        public Guid CascadeFromQuestionId { get; set; }

        public int RosterLevelDepthOfParentQuestion { get; set; }
    }
}