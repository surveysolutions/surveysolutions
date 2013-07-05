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
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "ChangeAssignment")]
    public class ChangeAssignmentCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeAssignmentCommand"/> class.
        /// </summary>
        /// <param name="completeQuestionnaireId">
        /// The complete questionnaire id.
        /// </param>
        /// <param name="responsible">
        /// The responsible.
        /// </param>
        public ChangeAssignmentCommand(Guid completeQuestionnaireId, UserLight responsible)
        {
            this.Responsible = responsible;
            this.CompleteQuestionnaireId = completeQuestionnaireId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        #endregion
    }
}