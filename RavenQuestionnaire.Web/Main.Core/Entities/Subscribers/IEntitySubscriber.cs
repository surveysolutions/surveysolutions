using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.Subscribers
{
    public interface IEntitySubscriber<T> : IDisposable 
    {
        void Subscribe(T target);
        void UnSubscribe(T target);
        bool IsSubscribed(T target);
    }
}
