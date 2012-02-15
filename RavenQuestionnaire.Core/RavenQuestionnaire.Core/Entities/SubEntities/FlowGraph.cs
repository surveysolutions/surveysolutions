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
        public FlowGraph()
        {
        this.observers=new List<IObserver<CompositeEventArgs>>();
        }

        public List<FlowBlock> Blocks { get; set; }
        public List<FlowConnection> Connections { get; set; }

        public Guid PublicKey
        {
            get { return Guid.Empty; }
        }

        public void Add(IComposite c, Guid? parent)
        {
            var block = c as FlowBlock;
            if (block != null)
            {
                Blocks.Add(block);
                return;
            }
            var connection = c as FlowConnection;
            if (connection != null)
            {
                Connections.Add(connection);
                return;
            }
            throw new CompositeException();
        }

        public void Remove(IComposite c)
        {
            foreach (FlowBlock child in Blocks)
            {
                if (child == c)
                {
                    Blocks.Remove(child);
                    return;
                }
                try
                {
                    child.Remove(c);
                    return;

                }
                catch (CompositeException)
                {

                }
            }
            foreach (FlowConnection child in Connections)
            {
                if (child == c)
                {
                    Connections.Remove(child);
                    return;
                }
                try
                {
                    child.Remove(c);
                }
                catch (CompositeException)
                {

                }
            }
            throw new CompositeException();
        }

        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            foreach (FlowBlock child in Blocks)
            {
                if (child.PublicKey == publicKey)
                {
                    Blocks.Remove(child);
                    return;
                }
                try
                {
                    child.Remove<T>(publicKey);
                    return;

                }
                catch (CompositeException)
                {
                }
            }
            foreach (FlowConnection child in Connections)
            {
                if ((child.Source == publicKey) || (child.Target == publicKey))
                {
                    Connections.Remove(child);
                    return;
                }
                try
                {
                    child.Remove<T>(publicKey);
                    return;
                }
                catch (CompositeException)
                {
                }
            }
            throw new CompositeException();
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return null;
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return new T[0];
        }

        #region Implementation of IObservable<out CompositeEventArgs>

        public IDisposable Subscribe(IObserver<CompositeEventArgs> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            return new Unsubscriber<CompositeEventArgs>(observers, observer);
        }
        private List<IObserver<CompositeEventArgs>> observers;

        #endregion
    }
}
