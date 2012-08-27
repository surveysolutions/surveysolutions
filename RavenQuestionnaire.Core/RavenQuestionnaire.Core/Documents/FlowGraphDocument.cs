using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents
{
    public interface IFlowGraphDocument
    {
        Guid PublicKey { get; set; }

        Guid QuestionnaireDocumentId { get; set; }

        DateTime CreationDate { get; set; }
        DateTime LastEntryDate { get; set; }

        List<FlowBlock> Blocks { get; set; }
        List<FlowConnection> Connections { get; set; }
    }

    public class FlowGraphDocument : IFlowGraphDocument
    {
        public FlowGraphDocument()
        {
            CreationDate = DateTime.Now;
            LastEntryDate = DateTime.Now;
            Blocks = new List<FlowBlock>();
            Connections = new List<FlowConnection>();
        }

        //public string Id { get; set; }

        public Guid PublicKey { get; set; }


        public Guid QuestionnaireDocumentId { get; set; }

        public List<FlowBlock> Blocks { get; set; }
        public List<FlowConnection> Connections { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }
    }
}
