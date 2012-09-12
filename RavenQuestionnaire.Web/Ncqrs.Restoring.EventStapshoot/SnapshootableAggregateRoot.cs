// -----------------------------------------------------------------------
// <copyright file="SnapshootableAggregateRoot.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Ncqrs.Eventing.Sourcing.Mapping;

namespace Ncqrs.Restoring.EventStapshoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SnapshootableAggregateRoot : MappedAggregateRoot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshootableAggregateRoot"/> class.
        /// </summary>
        protected SnapshootableAggregateRoot()
            : base(new SnapshootableEventHandlerMappingStrategy(new ConventionBasedEventHandlerMappingStrategy()))
        {
        }

        protected SnapshootableAggregateRoot(Guid id)
            : base(id, new SnapshootableEventHandlerMappingStrategy(new ConventionBasedEventHandlerMappingStrategy()))
        {
        }
    }

}
