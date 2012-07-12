using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "DeleteImage")]
    public class DeleteImageCommand : CommandBase
    {
        public DeleteImageCommand(Guid questionnaireId, Guid questionKey, Guid imageKey)
        {
            QuestionKey = questionKey;
            ImageKey = imageKey;
            QuestionnaireId = questionnaireId;
        }

        public Guid QuestionKey { get; set; }

        public Guid ImageKey { get; set; }
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }
       
    }
}