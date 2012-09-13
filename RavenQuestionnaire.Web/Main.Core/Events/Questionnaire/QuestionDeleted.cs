// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionDeleted.cs" company="">
//   
// </copyright>
// <summary>
//   The question deleted.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The question deleted.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionDeleted")]
    public class QuestionDeleted
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the question id.
        /// </summary>
        public Guid QuestionId { get; set; }

        #endregion
    }
}