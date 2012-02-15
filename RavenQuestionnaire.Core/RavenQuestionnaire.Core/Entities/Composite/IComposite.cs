using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.Composite
{
    public interface IComposite : IObservable<CompositeEventArgs>
    {
        Guid PublicKey { get; }
        void Add(IComposite c, Guid? parent);
        void Remove(IComposite c);
        void Remove<T>(Guid publicKey) where T : class, IComposite;
        T Find<T>(Guid publicKey) where T : class, IComposite;
        IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class;


    }
    public class Unsubscriber<T> : IDisposable
    {
        private List<IObserver<T>> _observers;
        private IObserver<T> _observer;

        public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
    /* public static class EventsProcessor
    {
       // private readonly List<Func<IObservable<object>, IObservable<object>>> actions = new List<Func<IObservable<object>, IObservable<object>>>();

        public static void On<T>(this IComposite composite, Func<IObservable<T>, IObservable<object>> action)
        {
            actions.Add(observable => action(observable.OfType<T>()));
        }

        public static void Execute(this IComposite composite, IObservable<object> observable)
        {
            foreach (var action in actions)
            {
                action(observable).Subscribe();
            }
        }
    }*/
    public abstract class CompositeEventArgs : EventArgs
    {
        protected CompositeEventArgs()
        {
        }

        protected CompositeEventArgs(CompositeEventArgs parent)
        {
            this.ParentEvent = parent;
        }

        public CompositeEventArgs ParentEvent { get; private set; }
    }

    public class CompositeAddedEventArgs : CompositeEventArgs
    {
         public CompositeAddedEventArgs(IComposite added)
         {
             this.AddedComposite = added;
         }

        public CompositeAddedEventArgs(CompositeEventArgs parent, IComposite added)
            : base(parent)
        {
            this.AddedComposite = added;
        }

        public IComposite AddedComposite { get; private set; }
    }

    public class CompositeRemovedEventArgs : CompositeEventArgs
    {
        public CompositeRemovedEventArgs(IComposite removed)
        {
            this.RemovedComposite = removed;
        }

        public CompositeRemovedEventArgs(CompositeEventArgs parent, IComposite removed)
            : base(parent)

        {
            this.RemovedComposite = removed;
        }
        public IComposite RemovedComposite { get; private set; }
    }
}
