using System.Collections.Generic;
using System.Linq;
using Main.Core.Events;
using Ncqrs.Eventing;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.SharedKernels.DataCollection.Events
{
    public static class AggregateRootEventExtensions
    {
        private static HashSet<string> ChangeEventsState =
            EventsThatChangeAnswersStateProvider.GetTypeNames()
                .Concat(EventsThatAssignInterviewToResponsibleProvider.GetTypeNames())
                .ToHashSet();

        public static AggregateRootEvent[] FilterDuplicateEvents(this AggregateRootEvent[] tabletEvents,
            List<CommittedEvent> hqEvents)
        {
            var tabletEventIds = tabletEvents.Select(e => e.EventIdentifier).Reverse();
            var hqEventIds = hqEvents.Select(e => e.EventIdentifier).Reverse();
            var lastCommonEventId = tabletEventIds.Intersect(hqEventIds).FirstOrDefault();
            if (lastCommonEventId == default)
                return tabletEvents;

            var filteredHqEvents = hqEvents.SkipWhile(e => e.EventIdentifier != lastCommonEventId).Skip(1).ToArray();
            if (filteredHqEvents.Any(e => ChangeEventsState.Contains(e.Payload.GetType().Name)))
            {
                throw new InterviewException(
                    "Found active event on hq/supervisor side. Cannot merge streams",
                    exceptionType: InterviewDomainExceptionType.PackageIsOudated);
            }

            var filteredTabletEvents =
                tabletEvents.SkipWhile(e => e.EventIdentifier != lastCommonEventId).Skip(1).ToArray();
            return filteredTabletEvents;
        }
    }
}
