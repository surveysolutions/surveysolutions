// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddPropagatableGroupCommand.cs" company="">
//   
// </copyright>
// <summary>
//   The add propagatable group command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The add propagatable group command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "AddPropagatableGroup")]
    public class AddPropagatableGroupCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AddPropagatableGroupCommand"/> class.
        /// </summary>
        /// <param name="completeQuestionnaireId">
        /// The complete questionnaire id.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        public AddPropagatableGroupCommand(Guid completeQuestionnaireId, Guid propagationKey, Guid publicKey)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;
            this.PublicKey = publicKey;
            this.PropagationKey = propagationKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the propagation key.
        /// </summary>
        public Guid PropagationKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        #endregion
    }
}