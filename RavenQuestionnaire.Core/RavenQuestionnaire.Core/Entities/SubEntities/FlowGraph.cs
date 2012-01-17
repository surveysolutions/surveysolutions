using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface IFlowGraph : IComposite
    {
    }
    public interface IFlowGraph<TFlowBlock, TFlowConnection> : IFlowGraph
        where TFlowBlock : IFlowBlock
        where TFlowConnection : IFlowConnection
    {
        List<TFlowBlock> Blocks { get; set; }
        List<TFlowConnection> Connections { get; set; }
    }
    public class FlowGraph : IFlowGraph<FlowBlock, FlowConnection>
    {
        public List<FlowBlock> Blocks { get; set; }
        public List<FlowConnection> Connections { get; set; }

        public bool Add(IComposite c, Guid? parent)
        {
            var block = c as FlowBlock;
            if (block != null)
            {
                Blocks.Add(block);
                return true;
            }
            var connection = c as FlowConnection;
            if (connection != null)
            {
                Connections.Add(connection);
                return true;
            }
            return false;
        }

        public bool Remove(IComposite c)
        {
            foreach (FlowBlock child in Blocks)
            {
                if (child == c)
                {
                    Blocks.Remove(child);
                    return true;
                }
                if (child.Remove(c))
                    return true;
            }
            foreach (FlowConnection child in Connections)
            {
                if (child == c)
                {
                    Connections.Remove(child);
                    return true;
                }
                if (child.Remove(c))
                    return true;
            }
            return false;
        }

        public bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            foreach (FlowBlock child in Blocks)
            {
                if (child.QuestionId == publicKey)
                {
                    Blocks.Remove(child);
                    return true;
                }
                if (child.Remove<T>(publicKey))
                    return true;
            }
            foreach (FlowConnection child in Connections)
            {
                if ((child.Source == publicKey) || (child.Target == publicKey))
                {
                    Connections.Remove(child);
                    return true;
                }
                if (child.Remove<T>(publicKey))
                    return true;
            }
            return false;
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return null;
        }
    }
}
