namespace Main.Core.Commands.Questionnaire.Completed
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// COmmand is used for status changing.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(_CompleteQuestionnaireAR), "ChangeStatus")]
    public class _ChangeStatusCommand : CommandBase
    {
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

      
        public SurveyStatus Status { get; set; }

   
        public UserLight Responsible { get; set; }
    }
}