// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageUpdated.cs" company="">
//   
// </copyright>
// <summary>
//   The image updated.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The image updated.
    /// </summary>
    [EventName("RavenQuestionnaire.Core:Events:ImageUploaded")]
    public class ImageUpdated
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the image key.
        /// </summary>
        public Guid ImageKey { get; set; }

        /// <summary>
        /// Gets or sets the question key.
        /// </summary>
        public Guid QuestionKey { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}