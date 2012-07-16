using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "DeleteQuestion")]
    public class DeleteQuestionCommand : CommandBase
    {
       
        public Guid QuestionId { get; set; }
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        public DeleteQuestionCommand(Guid questionId, Guid questionnaireId)
        {
            this.QuestionId = questionId;
            this.QuestionnaireId = questionnaireId;
        }
    }
}
