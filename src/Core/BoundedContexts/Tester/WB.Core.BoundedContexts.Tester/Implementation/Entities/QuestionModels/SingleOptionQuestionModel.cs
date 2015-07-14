using System.Collections.Generic;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels
{
    public class SingleOptionQuestionModel : BaseQuestionModel
    {
        public List<OptionModel> Options { get; set; }
    }
}