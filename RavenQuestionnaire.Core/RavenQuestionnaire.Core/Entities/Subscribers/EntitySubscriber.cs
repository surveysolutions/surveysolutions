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

        public abstract void Subscribe(T target);

        public void UnSubscribe(T target)
        {
            IDisposable subscriber;
            if (subsribers.TryGetValue(target.PublicKey, out subscriber))
            {
                subscriber.Dispose();
                subsribers.Remove(target.PublicKey);
            }
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
