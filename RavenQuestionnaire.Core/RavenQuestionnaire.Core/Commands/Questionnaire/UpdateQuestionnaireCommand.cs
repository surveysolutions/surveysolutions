// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateQuestionnaireCommand.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The update questionnaire command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    using RavenQuestionnaire.Core.Domain;

    /// <summary>
    /// The update questionnaire command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "UpdateQuestionnaire")]
    public class UpdateQuestionnaireCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateQuestionnaireCommand"/> class. 
        /// </summary>
        /// <param name="questionnaireId">
        /// </param>
        /// <param name="title">
        /// </param>
        public UpdateQuestionnaireCommand(Guid questionnaireId, string title)
        {
            this.QuestionnaireId = questionnaireId;
            this.Title = title;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}