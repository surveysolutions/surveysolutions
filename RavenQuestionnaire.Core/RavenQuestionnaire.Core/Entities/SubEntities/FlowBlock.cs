using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface IFlowBlock
    {
        int Left { get; set; }
        int Top { get; set; }
        int Height { get; set; }
        int Width { get; set; }
    }

    public class FlowBlock : IFlowBlock
    {
        public FlowBlock()
        {
        }

        public FlowBlock(Guid questionId)
        {
            PublicKey = questionId;
        }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public Guid PublicKey { get; set; }
    }
}
