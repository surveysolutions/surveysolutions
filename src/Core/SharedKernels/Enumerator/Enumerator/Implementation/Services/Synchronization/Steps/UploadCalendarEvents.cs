using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public class UploadCalendarEvents : SynchronizationStep
    {
        private readonly IEnumeratorEventStorage eventStorage;
        private readonly IJsonAllTypesSerializer synchronizationSerializer;
        private readonly ICalendarEventStorage calendarEventStorage;
        private readonly IPrincipal principal;

        public UploadCalendarEvents(int sortOrder, ISynchronizationService synchronizationService, 
            ILogger logger, IEnumeratorEventStorage eventStorage, 
            IJsonAllTypesSerializer synchronizationSerializer,
            ICalendarEventStorage calendarEventStorage,
            IPrincipal principal) 
            : base(sortOrder, synchronizationService, logger)
        {
            this.eventStorage = eventStorage;
            this.synchronizationSerializer = synchronizationSerializer;
            this.calendarEventStorage = calendarEventStorage;
            this.principal = principal;
        }

        public override async Task ExecuteAsync()
        {
            var calendarEvents = calendarEventStorage.GetNotSynchedCalendarEvents().ToList();

            var transferProgress = Context.Progress.AsTransferReport();
            
            foreach (var calendarEvent in calendarEvents)
            {
                
                Context.Progress.Report(new SyncProgressInfo
                {
                    Title = string.Format(EnumeratorUIResources.Synchronization_Upload_Title_Format,
                        EnumeratorUIResources.Synchronization_Upload_CalendarEvents_Text),
                    Description = string.Format(EnumeratorUIResources.Synchronization_Upload_Description_Format,
                        Context.Statistics.SuccessfullyUploadedCalendarEventsCount, 
                        calendarEvents.Count,
                        EnumeratorUIResources.Synchronization_Upload_CalendarEvents_Text),
                    Status = SynchronizationStatus.Upload,
                    Stage = SyncStage.UploadingCalendarEvents,
                    Statistics = Context.Statistics,

                    StageExtraInfo = new Dictionary<string, string>()
                    {
                        { "processedCount", Context.Statistics.SuccessfullyUploadedCalendarEventsCount.ToString() },
                        { "totalCount", calendarEvents.Count.ToString()}
                    }
                });
                
                
                var eventsToSend =  GetCalendarEventStream(calendarEvent.Id);
                
                var package = new CalendarEventPackageApiView()
                {
                    CalendarEventId = calendarEvent.Id,
                    Events = this.synchronizationSerializer.Serialize(eventsToSend),
                    MetaInfo = new CalendarEventMetaInfo()
                    {
                        ResponsibleId   = calendarEvent.UserId,
                        LastUpdateDateTime = eventsToSend.Last().EventTimeStamp,
                        InterviewId = calendarEvent.InterviewId,
                        AssignmentId = calendarEvent.AssignmentId
                    }
                };

                await this.synchronizationService.UploadCalendarEventAsync(
                    calendarEvent.Id,
                    package,
                    transferProgress,
                    Context.CancellationToken);

                calendarEventStorage.SetCalendarEventSyncedStatus(calendarEvent.Id, true);
                eventStorage.MarkAllEventsAsReceivedByHq(calendarEvent.Id);
                
                this.Context.Statistics.SuccessfullyUploadedCalendarEventsCount++;
            }
        }

        private ReadOnlyCollection<CommittedEvent> GetCalendarEventStream(Guid calendarEventId)
        {
            return this.eventStorage.Read(calendarEventId, 
                    this.eventStorage.GetLastEventKnownToHq(calendarEventId) + 1)
                .ToReadOnlyCollection();
        }
    }
}
