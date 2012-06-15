using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ninject;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.Subscribers
{
    public class Subscriber:ISubscriber
    {
         private IKernel container;

        public Subscriber(IKernel containe)
        {
            this.container = containe;
        }
        #region Implementation of ISubscriber

        public void Subscribe<T>(T entity) where T : IObservable<CompositeEventArgs>
        {
            ExecuteMethod(entity, "Subscribe");
        }

        public void UnSubscribe<T>(T entity) where T : IObservable<CompositeEventArgs>
        {
          /*  var subscribers = GetAllBindings<T>();
            foreach (IEntitySubscriber<T> entitySubscriber in subscribers)
            {
                entitySubscriber.UnSubscribe(entity);
            }*/
            ExecuteMethod(entity, "UnSubscribe");
        }

        #endregion

        protected IEnumerable<IEntitySubscriber<T>> ExecuteMethod<T>(T entity, string method) where T : IObservable<CompositeEventArgs>
        {
            Type[] types = typeof(T).GetInterfaces();
            var result = new List<IEntitySubscriber<T>>();
            foreach (Type type in types)
            {
                if (typeof (IObservable<CompositeEventArgs>).IsAssignableFrom(type))
                {
                    var genericType = typeof (IEntitySubscriber<>).MakeGenericType(type);
                    var subscribers=
                        container.GetAll(genericType);
                    foreach (object subscriber in subscribers)
                    {
                        MethodInfo methodInfo = genericType.GetMethod(method);
                        if (methodInfo != null)
                        {
                            object[] parametersArray = new object[] { entity };

                            //The invoke does NOT work it throws "Object does not match target type"             
                            methodInfo.Invoke(subscriber, parametersArray);
                        }
                    }
                }
            }
            return result;
        }
    }
}
