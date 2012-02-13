using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface IFlowConnection : IComposite
    {
        Guid Source { get; set; }
        Guid Target { get; set; }
        string LabelText { get; set; }
    }

    public class FlowConnection : IFlowConnection
    {
        public FlowConnection()
        {
        }

        public FlowConnection(Guid sourceId, Guid targetId)
        {
            Source = sourceId;
            Target = targetId;
        }
        public Guid Source { get; set; }
        public Guid Target { get; set; }
        public string LabelText { get; set; }

        public Guid PublicKey
        {
            get { return Guid.Empty; }
        }

        public void Add(IComposite c, Guid? parent)
        {
            throw new CompositeException("flow connection is not hierarchical");
        }

        public void Remove(IComposite c)
        {
            throw new CompositeException("flow connection is not hierarchical");
        }

        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            throw new CompositeException("flow connection is not hierarchical");
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return new T[0];
        }
    }
}
