namespace Main.Core.Commands.Questionnaire.Completed
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// Command to change responsible person
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(_CompleteQuestionnaireAR), "ChangeAssignment")]
    public class _ChangeAssignmentCommand : CommandBase
    {
       
        public _ChangeAssignmentCommand(Guid completeQuestionnaireId, UserLight responsible)
        {
            this.Responsible = responsible;
            this.CompleteQuestionnaireId = completeQuestionnaireId;
        }

        

     
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        public UserLight Responsible { get; set; }

      
    }
}