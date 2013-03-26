// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteGroupCommand.cs" company="">
//   
// </copyright>
// <summary>
//   The delete group command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Commands.Questionnaire.Group
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The delete group command.
    /// </summary>
    [Obsolete]
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "DeleteGroup")]
    public class DeleteGroupCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteGroupCommand"/> class.
        /// </summary>
        /// <param name="groupPublicKey">
        /// The group public key.
        /// </param>
        /// <param name="parentPublicKey">
        /// The parent public key.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        public DeleteGroupCommand(Guid groupPublicKey, Guid? parentPublicKey, Guid questionnaireId)
        {
            this.GroupPublicKey = groupPublicKey;
            this.QuestionnaireId = questionnaireId;
            this.ParentPublicKey = parentPublicKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the group public key.
        /// </summary>
        public Guid GroupPublicKey { get; private set; }

        /// <summary>
        /// Gets the parent public key.
        /// </summary>
        public Guid? ParentPublicKey { get; private set; }

        /// <summary>
        /// Gets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; private set; }

        #endregion
    }
}