using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "CloneTextListQuestion")]
    public class CloneTextListQuestionCommand : AbstractListQuestionCommand
    {
        public CloneTextListQuestionCommand(
            Guid questionnaireId, 
            Guid questionId, 
            Guid groupId, Guid 
            sourceQuestionId, 
            int targetIndex,
            string title, 
            string variableName, 
            bool isMandatory, 
            string condition,
           string instructions,
            Guid responsibleId,
            int? maxAnswerCount)
            : base(responsibleId, questionnaireId, questionId, title, variableName, isMandatory, condition, instructions, maxAnswerCount)
        {
            this.GroupId = groupId;
            this.SourceQuestionId = sourceQuestionId;
            this.TargetIndex = targetIndex;
        }

        public Guid GroupId { get; private set; }
        public Guid SourceQuestionId { get; private set; }
        public int TargetIndex { get; private set; }
    }
}
