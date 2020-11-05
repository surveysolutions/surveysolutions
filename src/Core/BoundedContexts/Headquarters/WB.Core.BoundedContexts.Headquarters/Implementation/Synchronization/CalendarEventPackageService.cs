using System;
using Main.Core.Events;
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

        public CalendarEventPackageService(ILogger logger, SyncSettings syncSettings)
        {
            this.logger = logger;
            this.syncSettings = syncSettings;
        }

        public void ProcessPackage(CalendarEventPackage calendarEventPackage)
        {
            try
            {
                InScopeExecutor.Current.Execute(serviceLocator =>
                {
                    var aggregateRootEvents = serviceLocator.GetInstance<IJsonAllTypesSerializer>()
                        .Deserialize<AggregateRootEvent[]>(calendarEventPackage.Events.Replace(@"\u0000", ""));

                    //validate stream
                    
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
