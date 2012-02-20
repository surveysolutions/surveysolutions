using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public class FlowGraphRepository: EntityRepository<FlowGraph, FlowGraphDocument>, IFlowGraphRepository
    {
        public FlowGraphRepository(IDocumentSession documentSession) : base(documentSession) { }

        protected override FlowGraph Create(FlowGraphDocument doc)
        {
            return new FlowGraph(doc);
        }
    }
}
