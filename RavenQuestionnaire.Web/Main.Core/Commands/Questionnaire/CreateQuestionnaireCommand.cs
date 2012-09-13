// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateQuestionnaireCommand.cs" company="">
//   
// </copyright>
// <summary>
//   The create questionnaire command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Commands.Questionnaire
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The create questionnaire command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(QuestionnaireAR))]
    public class CreateQuestionnaireCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQuestionnaireCommand"/> class.
        /// </summary>
        public CreateQuestionnaireCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQuestionnaireCommand"/> class.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire id.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        public CreateQuestionnaireCommand(Guid questionnaireId, string text)
            : base(questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
            this.Text = text;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public Guid QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text { get; set; }

        #endregion
    }
}