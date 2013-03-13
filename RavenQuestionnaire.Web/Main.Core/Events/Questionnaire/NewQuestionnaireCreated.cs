// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewQuestionnaireCreated.cs" company="">
//   
// </copyright>
// <summary>
//   The new questionnaire created.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The new questionnaire created.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewQuestionnaireCreated")]
    public class NewQuestionnaireCreated
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the created by.
        /// </summary>
        public Guid? CreatedBy { get; set; }

        #endregion
    }
}