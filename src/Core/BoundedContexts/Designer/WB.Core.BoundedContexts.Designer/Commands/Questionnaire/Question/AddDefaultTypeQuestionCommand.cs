using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class AddDefaultTypeQuestionCommand : QuestionCommand
    {
        public AddDefaultTypeQuestionCommand(
            Guid questionnaireId,
            Guid questionId,
            Guid parentGroupId,
            string title,
            Guid responsibleId)
            : base(questionnaireId, questionId, responsibleId)
        {
            this.Title = title;
            this.ParentGroupId = parentGroupId;
        }

        public string Title { get; private set; }
        public Guid ParentGroupId { get; private set; }
    }
}
