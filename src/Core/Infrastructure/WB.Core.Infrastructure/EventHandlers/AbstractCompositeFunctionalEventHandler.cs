using System;
using System.Collections.Concurrent;
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
            foreach (var handler in Handlers)
            {
                if(Handles(evt, handler))
                    return true;
            }

            return false;
        }

        protected bool Handles(IUncommittedEvent evt, ICompositeFunctionalPartEventHandler<TEntity, TStorage> handler)
        {
            return handleCache.GetOrAdd((evt.Payload.GetType(), handler.GetType()), key =>
            {
                var updateHandler = typeof(IUpdateHandler<,>).MakeGenericType(typeof(TEntity), key.eventType);
                return updateHandler.IsAssignableFrom(key.handler);
            });
        }

        static readonly ConcurrentDictionary<(Type eventType, Type handler), bool> handleCache = 
            new ConcurrentDictionary<(Type eventType, Type handler), bool>();

        static readonly ConcurrentDictionary<(Type eventHandler, Type eventType), MethodInfo> updateMethodsCache 
            = new ConcurrentDictionary<(Type eventHandler, Type eventType), MethodInfo>();

        protected override TEntity ApplyEventOnEntity(IPublishableEvent evt, Type eventType, TEntity currentState)
        {
            TEntity newState = currentState;

            foreach (var functionalEventHandler in Handlers)
            {
                if (!Handles(evt, functionalEventHandler))
                    continue;
                
                var update = updateMethodsCache.GetOrAdd((functionalEventHandler.GetType(), eventType), k => 
                    k.eventHandler.GetMethod("Update", new[] {typeof(TEntity), k.eventType}));

                newState = (TEntity) update?.Invoke(functionalEventHandler, new object[] { newState, this.CreatePublishedEvent(evt) });
            }

            return newState;
        }

        public override void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus)
        {
            var handlerMethodNames = new string[] { "Create", "Update", "Delete" };

            List<MethodInfo> handlers = new List<MethodInfo>();

            foreach (var handler in Handlers)
            {
                handlers.AddRange(handler.GetType().GetMethods().Where(m => handlerMethodNames.Contains(m.Name)));
            }

            handlers.ForEach((handler) => this.RegisterOldFashionHandler(oldEventBus, handler));
        }
    }
}
