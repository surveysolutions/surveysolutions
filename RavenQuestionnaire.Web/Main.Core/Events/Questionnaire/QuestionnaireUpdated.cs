// -----------------------------------------------------------------------
// <copyright file="QuestionnaireUpdated.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Ncqrs.Eventing.Storage;

namespace Main.Core.Events.Questionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireUpdated")]
    public class QuestionnaireUpdated
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        [Obsolete]
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        #endregion
    }
}
