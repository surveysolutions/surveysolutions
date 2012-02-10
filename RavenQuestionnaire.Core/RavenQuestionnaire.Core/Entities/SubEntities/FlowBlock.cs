using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface IFlowBlock : IComposite
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

        public void Add(IComposite c, Guid? parent)
        {
            throw new CompositeException("flow block is not hierarchical");
        }

        public void Remove(IComposite c)
        {
            throw new CompositeException("flow block is not hierarchical");
        }

        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            throw new CompositeException("flow block is not hierarchical");
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class, IComposite
        {
            return new T[0];
        }
    }
}
