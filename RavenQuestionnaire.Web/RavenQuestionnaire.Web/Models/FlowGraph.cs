using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Web.Models
{
    public class FlowGraph
    {
        public FlowGraph()
        {
            Blocks = new List<FlowBlock>();
            Connections = new List<FlowConnection>();
        }
        public List<FlowBlock> Blocks { get; set; }
        public List<FlowConnection> Connections { get; set; }
        public Guid? ParentPublicKey { get; set; }
    }
}
