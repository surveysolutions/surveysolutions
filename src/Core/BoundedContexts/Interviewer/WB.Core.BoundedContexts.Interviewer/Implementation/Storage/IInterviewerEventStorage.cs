using System;
using System.Collections.Generic;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Interviewer.Views;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Storage
{
    public interface IInterviewerEventStorage : IEventStore
    {
        void RemoveEventSourceById(Guid interviewId);

        /// <summary>
        /// Method for migration from old sqlite storage. should not be used in application and can be removed when all clients are updated to version 5.5
        /// </summary>
        /// <param name="eventViews"></param>
        void MigrateOldEvents(IEnumerable<EventView> eventViews);
    }
}