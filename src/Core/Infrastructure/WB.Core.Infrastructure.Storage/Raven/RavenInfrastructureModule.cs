﻿using System.Linq;
using Ninject.Modules;
using Raven.Client.Document;
using WB.Core.Infrastructure.Storage.Raven.Implementation;

namespace WB.Core.Infrastructure.Storage.Raven
{
    public abstract class RavenInfrastructureModule : NinjectModule
    {
        private readonly RavenConnectionSettings settings;

        protected RavenInfrastructureModule(RavenConnectionSettings settings)
        {
            this.settings = settings;
        }

        protected void BindDocumentStore()
        {
            if (this.IsDocumentStoreAlreadyBound())
                return;

            var storeProvider = new DocumentStoreProvider(this.settings);
            this.Bind<DocumentStoreProvider>().ToConstant(storeProvider);
            this.Bind<DocumentStore>().ToProvider<DocumentStoreProvider>();
        }

        private bool IsDocumentStoreAlreadyBound()
        {
            return this.Kernel.GetBindings(typeof(DocumentStore)).Any();
        }
    }
}