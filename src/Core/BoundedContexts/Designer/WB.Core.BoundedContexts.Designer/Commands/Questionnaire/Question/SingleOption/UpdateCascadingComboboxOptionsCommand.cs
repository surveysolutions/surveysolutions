using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.SingleOption
{
    [Serializable]
    public class UpdateCascadingComboboxOptionsCommand : QuestionCommand
    {
        public UpdateCascadingComboboxOptionsCommand(
            Guid questionnaireId,
            Guid questionId,
            Guid responsibleId,
            Option[] options)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId)
        {
            this.Options = options;
        }

        public Option[] Options { get; set; }
    }
}