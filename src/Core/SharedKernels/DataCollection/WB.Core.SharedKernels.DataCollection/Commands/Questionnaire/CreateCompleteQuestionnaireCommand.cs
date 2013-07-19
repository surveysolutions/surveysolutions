using System;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    /// <summary>
    /// The create complete questionnaire command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "CreateCompletedQ")]
    public class CreateCompleteQuestionnaireCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCompleteQuestionnaireCommand"/> class.
        /// </summary>
        /// <param name="completedQuestionnaireId">
        /// The completed questionnaire id.
        /// </param>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="creator">
        /// Person, who created survey
        /// </param>
        public CreateCompleteQuestionnaireCommand(Guid completedQuestionnaireId, Guid questionnaireId, UserLight creator)
        {
            this.CompleteQuestionnaireId = completedQuestionnaireId;
            this.QuestionnaireId = questionnaireId;
            this.Creator = creator;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        public Guid CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets Creator.
        /// </summary>
        public UserLight Creator { get; set; }

        #endregion
    }
}