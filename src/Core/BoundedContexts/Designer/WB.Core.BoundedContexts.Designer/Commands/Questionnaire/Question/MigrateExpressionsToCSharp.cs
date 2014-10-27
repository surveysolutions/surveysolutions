using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "MigrateExpressionsToCSharp")]
    public class MigrateExpressionsToCSharp : CommandBase
    {
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        public MigrateExpressionsToCSharp(Guid questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
        }
    }
}