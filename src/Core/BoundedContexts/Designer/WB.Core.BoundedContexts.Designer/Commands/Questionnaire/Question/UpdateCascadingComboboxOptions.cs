using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateCascadingComboboxOptions : QuestionCommand
    {
        public UpdateCascadingComboboxOptions(
            Guid questionnaireId,
            Guid questionId,
            Guid responsibleId,
            QuestionnaireCategoricalOption[] options)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId)
        {
            this.Options = options;
        }

        public QuestionnaireCategoricalOption[] Options { get; set; }
    }
}
