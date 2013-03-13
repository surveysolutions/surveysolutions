// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteQuestionnaireCommand.cs" company="">
//   
// </copyright>
// <summary>
//   The Delete questionnaire command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Commands.Questionnaire
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The Delete questionnaire command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "DeleteQuestionnaire")]
    public class DeleteQuestionnaireCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteQuestionnaireCommand"/> class. 
        /// </summary>
        /// <param name="questionnaireId">
        /// </param>
        public DeleteQuestionnaireCommand(Guid questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        #endregion
    }
}