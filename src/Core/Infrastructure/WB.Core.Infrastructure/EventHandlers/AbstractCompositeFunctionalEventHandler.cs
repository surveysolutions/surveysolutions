using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.EventHandlers
{
    public interface ICompositeFunctionalPartEventHandler<TEntity, TStorage>
        where TEntity : class, IReadSideRepositoryEntity
        where TStorage : class, IReadSideStorage<TEntity>
    { }


    public abstract class AbstractCompositeFunctionalEventHandler<TEntity, TStorage> : AbstractFunctionalEventHandler<TEntity, TStorage>
        where TEntity : class, IReadSideRepositoryEntity
        where TStorage : class, IReadSideStorage<TEntity>
    {
        public abstract ICompositeFunctionalPartEventHandler<TEntity, TStorage>[] Handlers { get; }

        protected AbstractCompositeFunctionalEventHandler(TStorage readSideStorage) : base(readSideStorage)
        {
        }

        protected override bool Handles(IUncommittedEvent evt)
        {
            Type genericUpgrader = typeof(IUpdateHandler<,>);
            var typeInfo = genericUpgrader.MakeGenericType(typeof(TEntity), evt.Payload.GetType()).GetTypeInfo();
            foreach (var handler in Handlers)
            {
                if (typeInfo.IsInstanceOfType(handler))
                    return true;
            }
            return false;
        }

        protected bool Handles(IUncommittedEvent evt, ICompositeFunctionalPartEventHandler<TEntity, TStorage> handler)
        {
            Type genericUpgrader = typeof(IUpdateHandler<,>);
            var typeInfo = genericUpgrader.MakeGenericType(typeof(TEntity), evt.Payload.GetType()).GetTypeInfo();
            return typeInfo.IsAssignableFrom(handler.GetType());
        }

        protected override TEntity ApplyEventOnEntity(IPublishableEvent evt, Type eventType, TEntity currentState)
        {
            TEntity newState = currentState;

            foreach (var functionalEventHandler in Handlers)
            {
                if (!Handles(evt, functionalEventHandler))
                    continue;

                newState = (TEntity)functionalEventHandler
                    .GetType()
                    .GetTypeInfo().GetMethod("Update", new[] { typeof(TEntity), eventType })
                    .Invoke(functionalEventHandler, new object[] { newState, this.CreatePublishedEvent(evt) });
            }

            return newState;
        }

        public override void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus)
        {
            var handlerMethodNames = new string[] { "Create", "Update", "Delete" };

            List<MethodInfo> handlers = new List<MethodInfo>();

            foreach (var handler in Handlers)
            {
                handlers.AddRange(handler.GetType().GetTypeInfo().GetMethods().Where(m => handlerMethodNames.Contains(m.Name)));
            }

            handlers.ForEach((handler) => this.RegisterOldFashionHandler(oldEventBus, handler));
        }
    }
}
