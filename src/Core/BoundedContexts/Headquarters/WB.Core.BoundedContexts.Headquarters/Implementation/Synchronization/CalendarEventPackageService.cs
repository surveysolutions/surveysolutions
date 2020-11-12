using System;
using Main.Core.Events;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Enumerator.Native.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization
{
    public class CalendarEventPackageService : ICalendarEventPackageService
    {
        private readonly ILogger logger;
        private readonly SyncSettings syncSettings;
        private readonly ICalendarEventService calendarEventService;

        public CalendarEventPackageService(ILogger logger, SyncSettings syncSettings, 
            ICalendarEventService calendarEventService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.syncSettings = syncSettings ?? throw new ArgumentNullException(nameof(syncSettings));
            this.calendarEventService = calendarEventService ?? throw new ArgumentNullException(nameof(calendarEventService));
        }

        public void ProcessPackage(CalendarEventPackage calendarEventPackage)
        {
            try
            {
                if(!calendarEventPackage.InterviewId.HasValue)
                    throw new InvalidOperationException("Calendar event package has no interview Id");
                InScopeExecutor.Current.Execute(serviceLocator =>
                {
                    var activeCalendarEventByInterviewId =
                        calendarEventService.GetActiveCalendarEventByInterviewId(calendarEventPackage
                            .InterviewId.Value);

                    //remove other older CE 
                    if (activeCalendarEventByInterviewId != null &&
                        activeCalendarEventByInterviewId.PublicKey != calendarEventPackage.CalendarEventId
                        && activeCalendarEventByInterviewId.UpdateDate < calendarEventPackage.LastUpdateDate)
                    {
                        serviceLocator.GetInstance<ICommandService>().Execute(
                            new DeleteCalendarEventCommand(activeCalendarEventByInterviewId.PublicKey,
                                calendarEventPackage.ResponsibleId),
                            this.syncSettings.Origin);
                    }
                    
                    var aggregateRootEvents = serviceLocator.GetInstance<IJsonAllTypesSerializer>()
                        .Deserialize<AggregateRootEvent[]>(calendarEventPackage.Events.Replace(@"\u0000", ""));

                    //validate stream
                    //merge if there are changes on server?

                    serviceLocator.GetInstance<ICommandService>().Execute(
                        new SyncCalendarEventEventsCommand(aggregateRootEvents,
                            calendarEventPackage.CalendarEventId,
                            calendarEventPackage.ResponsibleId),
                            this.syncSettings.Origin);
                });

            }
            catch (Exception exception)
            {
                this.logger.Error(
                    $"Calendar event events by {calendarEventPackage.CalendarEventId} processing failed. Reason: '{exception.Message}'",
                    exception);

                throw;
            }
        }
    }
}
