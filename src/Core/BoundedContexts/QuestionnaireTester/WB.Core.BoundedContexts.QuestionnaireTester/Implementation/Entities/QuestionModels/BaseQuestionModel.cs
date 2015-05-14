using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels
{
    public abstract class BaseQuestionModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public bool IsPrefilled { get; set; }
        public string Instructions { get; set; }
        public bool IsMandatory { get; set; }
        public string ValidationMessage { get; set; }
    }
}