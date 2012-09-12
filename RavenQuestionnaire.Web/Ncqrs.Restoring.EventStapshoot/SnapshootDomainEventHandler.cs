// -----------------------------------------------------------------------
// <copyright file="SnapshootDomainEventHandler.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Ncqrs.Eventing.Sourcing;

namespace Ncqrs.Restoring.EventStapshoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SnapshootDomainEventHandler : ISourcedEventHandler
    {
        public SnapshootDomainEventHandler(object target)
        {
            this.target = target;
        }

        private readonly object target;
        #region Implementation of ISourcedEventHandler

        public bool HandleEvent(object sourcedEvent)
        {
            var snapshootLoaded = sourcedEvent as SnapshootLoaded;
            if (snapshootLoaded == null)
                return false;
            var restoreFromSnapshotMethod = target.GetType().GetMethod("RestoreFromSnapshot");
            if (restoreFromSnapshotMethod == null)
               throw new AggregateException(
                    "aggregate root is not implementing ISnapshotable, but SnapshootDomainEventHandler is registred. That is impossible");
            restoreFromSnapshotMethod.Invoke(target, new[] {snapshootLoaded.Template.Payload});
            return true;

        }

        #endregion
    }
}
