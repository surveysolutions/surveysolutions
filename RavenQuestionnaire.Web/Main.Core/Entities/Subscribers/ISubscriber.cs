using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.Subscribers
{
    public interface ISubscriber
    {
        void Subscribe<T>(T entity) where T : IObservable<CompositeEventArgs>;
        void UnSubscribe<T>(T entity) where T : IObservable<CompositeEventArgs>;
    }
}
