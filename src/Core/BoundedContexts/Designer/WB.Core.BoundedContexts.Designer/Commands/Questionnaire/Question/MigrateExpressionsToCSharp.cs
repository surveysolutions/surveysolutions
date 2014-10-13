using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "MigrateExpressionsToCSharp")]
    public class MigrateExpressionsToCSharp : QuestionnaireCommand
    {
        public MigrateExpressionsToCSharp(Guid questionnaireId, Guid responsibleId)
            : base(questionnaireId, responsibleId) {}
    }
}