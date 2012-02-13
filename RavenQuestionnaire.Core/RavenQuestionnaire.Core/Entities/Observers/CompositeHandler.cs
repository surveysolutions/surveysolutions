using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.Observers
{
    public class CompositeHandler : IObservable<CompositeInfo>
    {
        public List<IObserver<CompositeInfo>> Observers { get; set; }
        private IComposite document;
    //    private List<CompositeInfo> flights;
        public CompositeHandler( IComposite document)
        {
            Observers = new List<IObserver<CompositeInfo>>();
            SetDocument(document);
        }

        public CompositeHandler(List<IObserver<CompositeInfo>> observers, IComposite document):this(document)
        {
            this.Observers = observers;
            foreach (GroupObserver observer in Observers)
            {
                observer.Subscribe(this);
            }
            //flights = new List<CompositeInfo>();
        }

        #region Implementation of IObservable<out IComposite>

        public IDisposable Subscribe(IObserver<CompositeInfo> observer)
        {
            if (!Observers.Contains(observer))
            {
                Observers.Add(observer);
           /*     // Provide observer with existing data.
                foreach (var item in flights)
                    observer.OnNext(item);*/
            }
            return new Unsubscriber<CompositeInfo>(Observers, observer);
        }

        #endregion
        public void SetDocument(IComposite document)
        {
            this.document = document;
        }

        public void Add(IComposite target)
        {
            foreach (IObserver<CompositeInfo> observer in Observers)
            {
                observer.OnNext(new CompositeInfo() {Action = Actions.Add, Target = target, Document = document});
            }
        }
        public void Remove(IComposite target)
        {
            foreach (IObserver<CompositeInfo> observer in Observers)
            {
                observer.OnNext(new CompositeInfo() { Action = Actions.Remove, Target = target, Document = document });
            }
        }
        public void Remove<T>(Guid publicKey) where T :class,  IComposite
        {
            var target = document.Find<T>(publicKey);
            foreach (IObserver<CompositeInfo> observer in Observers)
            {
                observer.OnNext(new CompositeInfo() {Action = Actions.Remove, Target = target, Document = document});
            }
        }
    }
    internal class Unsubscriber<T> : IDisposable
    {
        private List<IObserver<T>> _observers;
        private IObserver<T> _observer;

        internal Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }

}
