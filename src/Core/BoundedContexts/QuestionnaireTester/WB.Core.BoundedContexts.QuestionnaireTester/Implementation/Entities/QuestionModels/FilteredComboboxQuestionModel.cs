using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels
{
    public class FilteredComboboxQuestionModel : BaseQuestionModel
    {
        //public Guid? CascadeFromQuestionId { get; set; }

        public List<OptionModel> Options { get; set; }
    }
}