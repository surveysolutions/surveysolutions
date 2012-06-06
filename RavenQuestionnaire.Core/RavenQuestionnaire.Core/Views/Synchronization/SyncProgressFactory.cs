using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Synchronization
{
    public class SyncProgressFactory: IViewFactory<SyncProgressInputModel, SyncProgressView>
    {
        private IDocumentSession documentSession;

        public SyncProgressFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public SyncProgressView Load(SyncProgressInputModel input)
        {
            var process = this.documentSession.Load<SyncProcessDocument>(input.ProcessKey.ToString());
            return new SyncProgressView(process);
        }
    }
}
