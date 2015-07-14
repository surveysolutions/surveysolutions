using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels
{
    public class CascadingSingleOptionQuestionModel : BaseQuestionModel
    {
        public List<CascadingOptionModel> Options { get; set; }

        public Guid CascadeFromQuestionId { get; set; }

        public int RosterLevelDepthOfParentQuestion { get; set; }
    }
}