using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateFilteredComboboxOptions : QuestionCommand
    {
        public UpdateFilteredComboboxOptions(
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
