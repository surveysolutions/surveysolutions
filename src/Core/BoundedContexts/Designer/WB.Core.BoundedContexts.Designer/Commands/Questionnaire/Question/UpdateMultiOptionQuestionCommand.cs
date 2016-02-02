using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateMultiOptionQuestionCommand : UpdateValidatableQuestionCommand
    {
        public UpdateMultiOptionQuestionCommand(
            Guid questionnaireId,
            Guid questionId,
            string title,
            string variableName, string variableLabel,
            string enablementCondition,
            string instructions,
            Guid responsibleId,
            string validationExpression,
            string validationMessage,
            QuestionScope scope,
            Option[] options,
            Guid? linkedToQuestionId,
            bool areAnswersOrdered,
            int? maxAllowedAnswers,
            bool yesNoView,
            List<ValidationCondition> validationConditions)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, enablementCondition: enablementCondition, instructions: instructions,variableLabel:variableLabel,
                validationConditions: validationConditions)
        {
            this.Scope = scope;
            options?.ToList()
                .ForEach(x => x.Title = CommandUtils.SanitizeHtml(x.Title, removeAllTags: true));
            this.Options = options;
            this.LinkedToQuestionId = linkedToQuestionId;
            this.AreAnswersOrdered = areAnswersOrdered;
            this.MaxAllowedAnswers = maxAllowedAnswers;
            this.YesNoView = yesNoView;
        }

        public QuestionScope Scope { get; set; }

        public bool AreAnswersOrdered { get; set; }

        public Guid? LinkedToQuestionId { get; set; }

        public int? MaxAllowedAnswers { get; set; }

        public bool YesNoView { get; set; }

        public Option[] Options { get; set; }
    }
}