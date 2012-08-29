// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionMoved.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire item moved.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The questionnaire item moved.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireItemMoved")]
    public class QuestionnaireItemMoved
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the after item key.
        /// </summary>
        public Guid? AfterItemKey { get; set; }

        /// <summary>
        /// Gets or sets the group key.
        /// </summary>
        public Guid? GroupKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        public string QuestionnaireId { get; set; }

        #endregion
    }
}