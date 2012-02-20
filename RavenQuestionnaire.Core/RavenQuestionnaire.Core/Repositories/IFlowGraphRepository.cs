using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public interface IFlowGraphRepository : IEntityRepository<FlowGraph, FlowGraphDocument>
    {
    }
}
