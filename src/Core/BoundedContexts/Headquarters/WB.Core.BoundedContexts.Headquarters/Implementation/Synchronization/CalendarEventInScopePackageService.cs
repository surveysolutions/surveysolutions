#nullable enable
using System;
using System.Linq;
using Main.Core.Events;
using Microsoft.Extensions.Logging;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Enumerator.Native.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization
{
    public class CalendarEventInScopePackageService : ICalendarEventInScopePackageService
    {
        private readonly ILogger<CalendarEventInScopePackageService> logger;

        public CalendarEventInScopePackageService(ILogger<CalendarEventInScopePackageService> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void ProcessPackage(CalendarEventPackage calendarEventPackage)
        {
            try
            {
                InScopeExecutor.Current.Execute(serviceLocator =>
                {
                    var calendarEventPackageService = serviceLocator.GetInstance<ICalendarEventPackageService>();
                    calendarEventPackageService.ProcessPackage(calendarEventPackage);
                });
            }
            catch (Exception exception)
            {
                this.logger.LogError(
                    exception,
                    $"Calendar event events by {calendarEventPackage.CalendarEventId} processing failed. Reason: '{exception.Message}'");
                throw;
            }
        }
    }
}
