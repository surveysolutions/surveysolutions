using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    /// <summary>
    /// COmmand is used for status changing.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "ChangeStatus")]
    public class ChangeStatusCommand : CommandBase
    {
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }
        
        public SurveyStatus Status{get;set;}
    }
}
