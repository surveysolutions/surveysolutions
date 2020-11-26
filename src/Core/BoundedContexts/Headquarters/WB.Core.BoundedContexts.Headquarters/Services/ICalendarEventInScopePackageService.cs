using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ICalendarEventInScopePackageService
    {
        void ProcessPackage(CalendarEventPackage calendarEventPackage);
    }
}