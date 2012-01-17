using System;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class FlowBlock 
    {
        public FlowBlock(Guid id)
        {
            Id = id;
        }
        
        public int Left { get; set; }
        public int Top { get; set; }
        public Guid Id { get; set; }
    }
}
