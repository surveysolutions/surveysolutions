// -----------------------------------------------------------------------
// <copyright file="DocumentStoreProvider.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace RavenQuestionnaire.Core
{
    using System;

    using Ninject.Activation;

    using Raven.Client.Document;
    using Raven.Client.Embedded;
   
    //ref to dll
    using Raven.Database;
    using Raven.Storage.Esent;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DocumentStoreProvider : Provider<DocumentStore>
    {
        #region Fields

        /// <summary>
        /// The _is embeded.
        /// </summary>
        private readonly bool isEmbeded;

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
        /// <param name="isEmbeded">
        /// The is embeded.
        /// </param>
        public DocumentStoreProvider(string storage, bool isEmbeded)
        {
            this.storage = storage;
            this.isEmbeded = isEmbeded;
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
            DocumentStore store = null;
            try
            {
                if (this.isEmbeded)
                {
                    store = new EmbeddableDocumentStore
                    {
                        DataDirectory = this.storage
                    };
                }
                else
                {
                    store = new DocumentStore { Url = this.storage };
                }

                store.Initialize();
            }
            catch (Exception ex)
            {
                throw;// new Exception(ex.Message, ex);
            }

            return store;
        }

        #endregion
    }
}
