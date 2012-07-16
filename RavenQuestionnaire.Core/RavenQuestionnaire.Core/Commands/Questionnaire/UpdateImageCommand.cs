using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "UpdateImage")]
    public class UpdateImageCommand : CommandBase
    {
        public UpdateImageCommand(string questionnaireId, Guid questionKey, Guid imageKey, string title, string desc)
        {
            QuestionKey = questionKey;
            ImageKey = imageKey;
            QuestionnaireId = questionnaireId;
            Title = title;
            Description = desc;
        }
        
        public Guid QuestionKey { get; set; }

        public Guid ImageKey { get; set; }
        [AggregateRootId]
        public string QuestionnaireId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
    }
}