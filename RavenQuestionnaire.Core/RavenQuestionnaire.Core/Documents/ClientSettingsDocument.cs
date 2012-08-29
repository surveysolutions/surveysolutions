// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientSettingsDocument.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The client settings document.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Documents
{
    using System;

    /// <summary>
    /// The client settings document.
    /// </summary>
    public class ClientSettingsDocument
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        #endregion
    }
}