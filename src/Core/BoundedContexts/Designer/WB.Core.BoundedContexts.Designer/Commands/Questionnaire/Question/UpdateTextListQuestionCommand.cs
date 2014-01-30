using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "UpdateTextListQuestion")]
    public class UpdateTextListQuestionCommand : AbstractListQuestionCommand
    {
        public UpdateTextListQuestionCommand(
            Guid questionnaireId, 
            Guid questionId,
            string title, 
            string variableName, 
            bool isMandatory,
            string condition,
            string instructions, 
            Guid responsibleId, 
            int? maxAnswerCount)
            : base(responsibleId, questionnaireId, questionId, title, variableName, isMandatory, condition, instructions, maxAnswerCount) {}
    }
}
