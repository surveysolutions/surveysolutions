using System;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing.Sourcing.Snapshotting;

using WB.Core.GenericSubdomains.Portable.Services;

namespace Ncqrs.Domain
{
    internal static class AggregateRootExtensions
    {
        public static Type GetSnapshotInterfaceType(this Type aggregateType)
        {
            // Query all ISnapshotable interfaces. We only allow only
            // one ISnapshotable interface per aggregate root type.
            var snapshotables = from i in aggregateType.GetTypeInfo().ImplementedInterfaces
                                where i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(ISnapshotable<>)
                                select i;

            // Aggregate does not implement any ISnapshotable interface.
            if (snapshotables.Count() == 0)
            {
                return null;
            }
            // Aggregate does implement multiple ISnapshotable interfaces.
            if (snapshotables.Count() > 1)
            {
                return null;
            }

            var snapshotableInterfaceType = snapshotables.Single();

            return snapshotableInterfaceType;
        }
    }
}
