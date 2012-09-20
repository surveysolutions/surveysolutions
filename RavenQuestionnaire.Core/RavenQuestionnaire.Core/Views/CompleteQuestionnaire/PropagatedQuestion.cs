// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropagatedQuestion.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The propagated question.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View.Question;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;

    using RavenQuestionnaire.Core.Views.Question;

    /// <summary>
    /// The propagated question.
    /// </summary>
    public class PropagatedQuestion
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the instructions.
        /// </summary>
        public string Instructions { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        public string QuestionText { get; set; }

        /// <summary>
        /// Gets or sets the questions.
        /// </summary>
        public List<CompleteQuestionView> Questions { get; set; }

        #endregion
    }
}