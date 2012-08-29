// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientSettings.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The client settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities
{
    using System;

    using RavenQuestionnaire.Core.Documents;

    /// <summary>
    /// The client settings.
    /// </summary>
    public class ClientSettings : IEntity<ClientSettingsDocument>
    {
        #region Fields

        /// <summary>
        /// The inner document.
        /// </summary>
        private readonly ClientSettingsDocument innerDocument;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSettings"/> class.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        public ClientSettings(ClientSettingsDocument document)
        {
            this.innerDocument = document;

            // throw new InvalidOperationException("can't be bellow zero");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSettings"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        public ClientSettings(Guid publicKey)
        {
            this.innerDocument = new ClientSettingsDocument();
            this.innerDocument.PublicKey = publicKey;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the client settings public key.
        /// </summary>
        public Guid ClientSettingsPublicKey
        {
            get
            {
                return this.innerDocument.PublicKey;
            }
        }

        #endregion

        #region Explicit Interface Methods

        /// <summary>
        /// The get inner document.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.ClientSettingsDocument.
        /// </returns>
        ClientSettingsDocument IEntity<ClientSettingsDocument>.GetInnerDocument()
        {
            return this.innerDocument;
        }

        #endregion
    }
}