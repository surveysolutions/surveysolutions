using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public class SyncProcessRepository : EntityRepository<SyncProcess, SyncProcessDocument>, ISyncProcessRepository
    {
        public SyncProcessRepository(IDocumentSession documentSession) : base(documentSession) { }

        protected override SyncProcess Create(SyncProcessDocument doc)
        {
            return new SyncProcess(doc);
        }
        public override void Remove(SyncProcess entity)
        {
            throw new InvalidOperationException("User can't be deleted");
        }
    }
}
