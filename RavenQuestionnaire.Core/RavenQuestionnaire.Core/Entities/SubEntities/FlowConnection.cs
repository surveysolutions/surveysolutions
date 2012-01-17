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

        public bool Add(IComposite c, Guid? parent)
        {
            return false;
        }

        public bool Remove(IComposite c)
        {
            return false;
        }

        public bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            return false;
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return null;
        }
    }
}
