// -----------------------------------------------------------------------
// <copyright file="SnapshootableEventHandlerMappingStrategy.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Sourcing.Mapping;
using Ncqrs.Eventing.Sourcing.Snapshotting;

namespace Ncqrs.Restoring.EventStapshoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SnapshootableEventHandlerMappingStrategy : IEventHandlerMappingStrategy
    {
        private readonly IEventHandlerMappingStrategy baseStrategy;
        public SnapshootableEventHandlerMappingStrategy(IEventHandlerMappingStrategy stategy)
        {
            this.baseStrategy = stategy;
        }

        #region Implementation of IEventHandlerMappingStrategy

        public IEnumerable<ISourcedEventHandler> GetEventHandlers(object target)
        {
            var result = baseStrategy.GetEventHandlers(target).ToList();
            var targetType = target.GetType();
            // Query all ISnapshotable interfaces. We only allow only
            // one ISnapshotable interface per aggregate root type.
            var snapshotables = from i in targetType.GetInterfaces()
                                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof (ISnapshotable<>)
                                select i;
            if (!snapshotables.Any())
                throw new AggregateException("Aggreagate root must be snapsootable in case it uses SnapshootableEventHandlerMappingStrategy");

            var handlerForSnapshoots = new SnapshootDomainEventHandler(target);
            result.Add(handlerForSnapshoots);
            return result;
        }

        #endregion
    }
}
