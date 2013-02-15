// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentStoreProvider.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
#if !MONODROID

namespace Main.Core
{
    using System;

    using Ninject.Activation;

    using Raven.Client.Document;
    using Raven.Client.Embedded;

    /// <summary>
    /// The document store provider.
    /// </summary>
    public class DocumentStoreProvider : Provider<DocumentStore>
    {
        #region Fields

        /// <summary>
        /// The is embedded.
        /// </summary>
        private readonly bool isEmbedded;

        /// <summary>
        /// The _storage.
        /// </summary>
        private readonly string storage;


        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentStoreProvider"/> class.
        /// </summary>
        /// <param name="storage">
        /// The storage.
        /// </param>
        /// <param name="isEmbedded">
        /// The is embedded.
        /// </param>
        public DocumentStoreProvider(string storage, bool isEmbedded)
        {
            this.storage = storage;
            this.isEmbedded = isEmbedded;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The create instance.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The Raven.Client.Document.DocumentStore.
        /// </returns>
        protected override DocumentStore CreateInstance(IContext context)
        {
            DocumentStore store;

            store = this.isEmbedded ?
                        this.GetEmbededStorage() :
                        new DocumentStore { Url = this.storage };

            store.Initialize();

            return store;
        }
        protected EmbeddableDocumentStore GetEmbededStorage()
        {
            if (!isEmbedded)
                throw new InvalidOperationException("You can't call this method");
            if (embStorage == null || embStorage.WasDisposed)
            {
                embStorage = new EmbeddableDocumentStore() { DataDirectory = this.storage, UseEmbeddedHttpServer = false };
                embStorage.ResourceManagerId = Guid.NewGuid();
            }
            return embStorage;
        }
        private EmbeddableDocumentStore embStorage;
        #endregion
    }
}
#endif
