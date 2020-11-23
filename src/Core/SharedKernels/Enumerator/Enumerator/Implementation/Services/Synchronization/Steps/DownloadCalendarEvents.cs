#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public class DownloadCalendarEvents : SynchronizationStep
    {
        protected readonly IEnumeratorEventStorage EventStore;
        private readonly ILiteEventBus eventBus;
        private readonly ICalendarEventStorage calendarEventStorage;
        private readonly ILogger log;
        
        public DownloadCalendarEvents(int sortOrder, ISynchronizationService synchronizationService, 
            ILogger logger, IEnumeratorEventStorage eventStore,
            ICalendarEventStorage calendarEventStorage, ILiteEventBus eventBus, ILogger log) 
            : base(sortOrder, synchronizationService, logger)
        {
            EventStore = eventStore;
            this.eventBus = eventBus;
            this.log = log;
            this.calendarEventStorage = calendarEventStorage;
        }

        public override async Task ExecuteAsync()
        {
            List<CalendarEventApiView> remoteCalendarEvents = 
                await this.synchronizationService.GetCalendarEventsAsync(this.Context.CancellationToken);
            
            log.Debug($"Server has {remoteCalendarEvents.Count} calendar events");

            var remoteCalendarEventsWithSequence = 
                remoteCalendarEvents.ToDictionary(x => x.CalendarEventId, 
                    x => x.Sequence);
            
            // all local created should be knows on server at this point
            // 1. delete all not from list
            // 2. if server last event sequence is bigger than local
            //    drop local and recreate
            // 3. download all not existing locally or needed to be recreated
            
            var localCalendarEvents = calendarEventStorage.LoadAll();
            var localCalendarEventIds = localCalendarEvents.Select(ce => ce.Id).ToHashSet();

            var localCalendarEventsToRemoveIds = localCalendarEventIds.Where(
                ce => !remoteCalendarEventsWithSequence.ContainsKey(ce)).ToList(); 
            
            //all calendar events should be pushed at this point
            var localCalendarEventsToRecreateIds = localCalendarEventIds.Where(
                ce => remoteCalendarEventsWithSequence.ContainsKey(ce) &&
                      EventStore.GetLastEventSequence(ce) != remoteCalendarEventsWithSequence[ce]).ToList();

            var remoteCalendarEventsToDownloadIds = remoteCalendarEventsWithSequence.Keys.Where(
                ce => !localCalendarEventIds.Contains(ce)).ToList();
            
            IProgress<TransferProgress> transferProgress = this.Context.Progress.AsTransferReport();
            
            foreach (var localCalendarEventId in localCalendarEventsToRemoveIds)
            {
                this.RemoveCalendarEvent(localCalendarEventId);
            }
            var calendarEventsToProcess =
                localCalendarEventsToRecreateIds
                    .Union(remoteCalendarEventsToDownloadIds)
                    .Distinct().ToList();
            await DownloadAndCreateCalendarEvent(calendarEventsToProcess);
        }

        private void RemoveCalendarEvent(Guid id)
        {
            log.Debug($"Removing Calendar Event {id}");
            calendarEventStorage.Remove(id);
            EventStore.RemoveEventSourceById(id);
        }

        private async Task DownloadAndCreateCalendarEvent(List<Guid> calendarEventsIds)
        {
            var statistics = this.Context.Statistics;
            var progress = this.Context.Progress;
            var transferProgress = progress.AsTransferReport();
                
            foreach (var id in calendarEventsIds)
            {
                try
                {
                    this.Context.CancellationToken.ThrowIfCancellationRequested();
                    
                    this.RemoveCalendarEvent(id);
                    
                    progress.Report(new SyncProgressInfo
                    {
                        Title = EnumeratorUIResources.Synchronization_Download_CalendarEvents_Title,
                        Description = string.Format(EnumeratorUIResources.Synchronization_Download_Description_Format,
                            Context.Statistics.SuccessfullyDownloadedCalendarEventsCount, 
                            calendarEventsIds.Count,
                            EnumeratorUIResources.Synchronization_Upload_CalendarEvents_Text),
                        Stage = SyncStage.DownloadingCalendarEvents,
                        StageExtraInfo = new Dictionary<string, string>()
                        {
                            { "processedCount", Context.Statistics.SuccessfullyDownloadedCalendarEventsCount.ToString() },
                            { "totalCount", calendarEventsIds.Count.ToString()}
                        }
                    });

                    var calendarEventStream = await this.synchronizationService.GetCalendarEventStreamAsync(
                        id,
                        0,
                        transferProgress, 
                        Context.CancellationToken);
                
                    this.log.Debug($"Creating calendar event {id}");
                
                    EventStore.StoreEvents(new CommittedEventStream(id, calendarEventStream));
                    eventBus.PublishCommittedEvents(calendarEventStream);
                
                    calendarEventStorage.SetCalendarEventSyncedStatus(id, true);
                    Context.Statistics.SuccessfullyDownloadedCalendarEventsCount++;
                }
                catch (OperationCanceledException)
                {

                }
            }
            
            
        }
    }
}
