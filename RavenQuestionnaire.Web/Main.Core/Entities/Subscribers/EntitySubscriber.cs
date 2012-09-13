using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.Subscribers
{
    public abstract class EntitySubscriber<T> : IEntitySubscriber<T> where T : IComposite
    {
        protected Dictionary<Guid, IDisposable> subsribers = new Dictionary<Guid, IDisposable>();

        #region Implementation of IDisposable

        public  void Subscribe(T target)
        {
            if(IsSubscribed(target))
                return;
            subsribers.Add(target.PublicKey, GetUnsubscriber(target));
        }

        protected abstract IDisposable GetUnsubscriber(T target);

        public void UnSubscribe(T target)
        {
            IDisposable subscriber;
            if (subsribers.TryGetValue(target.PublicKey, out subscriber))
            {
                subscriber.Dispose();
                subsribers.Remove(target.PublicKey);
            }
        }

        public bool IsSubscribed(T target)
        {
            return subsribers.ContainsKey(target.PublicKey);
        }


        public void Dispose()
        {
            foreach (IDisposable disposable in subsribers.Values)
            {
                disposable.Dispose();
            }
            subsribers.Clear();
        }

        #endregion

    }
}
