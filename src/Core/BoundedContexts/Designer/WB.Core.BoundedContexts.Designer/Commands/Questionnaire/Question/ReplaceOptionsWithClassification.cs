using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    public class ReplaceOptionsWithClassification : QuestionCommand
    {
        public ReplaceOptionsWithClassification(Guid questionnaireId, Guid questionId, Guid classificationId, Guid responsibleId) : base(questionnaireId, questionId, responsibleId)
        {
            this.ClassificationId = classificationId;
        }

        public Guid ClassificationId { get; set; }

        public Option[] Options { get; set; } = new Option[0];
    }
}
