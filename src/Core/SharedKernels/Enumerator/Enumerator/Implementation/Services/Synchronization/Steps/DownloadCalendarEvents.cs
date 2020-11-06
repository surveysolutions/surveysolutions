using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public class DownloadCalendarEvents : SynchronizationStep
    {
        protected readonly IEnumeratorEventStorage EventStore;
        private readonly ILiteEventBus eventBus;
        
        public DownloadCalendarEvents(int sortOrder, ISynchronizationService synchronizationService, 
            ILogger logger, IEnumeratorEventStorage eventStore, ILiteEventBus eventBus) 
            : base(sortOrder, synchronizationService, logger)
        {
            EventStore = eventStore;
            this.eventBus = eventBus;
        }

        public override async Task ExecuteAsync()
        {
            List<CalendarEventApiView> remoteCalendarEvents = 
                await this.synchronizationService.GetCalendarEventsAsync(this.Context.CancellationToken);

            IProgress<TransferProgress> transferProgress = Context.Progress.AsTransferReport();
            foreach (var calendarEvent in remoteCalendarEvents)
            {
                try
                {
                    var calendarEventStream = await this.synchronizationService.GetCalendarEventStreamAsync(
                        calendarEvent.CalendarEventId,
                        0,
                        transferProgress, 
                        Context.CancellationToken);
                
                    EventStore.StoreEvents(new CommittedEventStream(calendarEvent.CalendarEventId, calendarEventStream));
                    eventBus.PublishCommittedEvents(calendarEventStream);
                }
                catch (OperationCanceledException)
                {

                }
            }
        }
    }
}
