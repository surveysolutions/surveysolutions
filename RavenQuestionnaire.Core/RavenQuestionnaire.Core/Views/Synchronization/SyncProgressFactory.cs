using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.ViewSnapshot;

namespace RavenQuestionnaire.Core.Views.Synchronization
{
    public class SyncProgressFactory: IViewFactory<SyncProgressInputModel, SyncProgressView>
    {
        private readonly IViewSnapshot store;

        public SyncProgressFactory(IViewSnapshot store)
        {
            this.store = store;
        }

        public SyncProgressView Load(SyncProgressInputModel input)
        {
            var process = this.store.ReadByGuid<SyncProcessDocument>(input.ProcessKey);
            return new SyncProgressView(process);
        }
    }
}
