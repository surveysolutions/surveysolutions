using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels
{
    public class FilteredSingleOptionQuestionModel : BaseQuestionModel
    {
        public List<OptionModel> Options { get; set; }
    }
}