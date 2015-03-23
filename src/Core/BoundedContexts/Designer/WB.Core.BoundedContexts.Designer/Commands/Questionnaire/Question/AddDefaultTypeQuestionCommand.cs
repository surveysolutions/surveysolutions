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
            Guid responsibleId,
            int? index = null)
            : base(questionnaireId, questionId, responsibleId)
        {
            this.Title = title;
            this.ParentGroupId = parentGroupId;
            this.Index = index;
        }

        public string Title { get; private set; }
        public Guid ParentGroupId { get; private set; }
        public int? Index { get; private set; }
    }
}
