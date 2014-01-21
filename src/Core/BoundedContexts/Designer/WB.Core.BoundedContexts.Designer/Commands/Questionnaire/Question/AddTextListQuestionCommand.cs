using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "AddTextListQuestion")]
    public class AddTextListQuestionCommand : AbstractListQuestionCommand
    {
        public AddTextListQuestionCommand(Guid questionnaireId, Guid questionId, Guid groupId,
            string title, string variableName,
            bool isMandatory, string condition, string instructions,
            Guid responsibleId, int? maxAnswerCount)
            : base(responsibleId, questionnaireId, questionId, title, variableName, isMandatory, condition, instructions, maxAnswerCount)
        {
            this.GroupId = groupId;
        }

        public Guid GroupId { get; private set; }
    }
}
