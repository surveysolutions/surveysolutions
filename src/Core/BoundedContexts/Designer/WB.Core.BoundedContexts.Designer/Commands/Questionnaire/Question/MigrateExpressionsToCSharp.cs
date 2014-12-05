using System;
using Ncqrs.Commanding;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    public class MigrateExpressionsToCSharp : CommandBase
    {
        public Guid QuestionnaireId { get; set; }

        public MigrateExpressionsToCSharp(Guid questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
        }
    }
}