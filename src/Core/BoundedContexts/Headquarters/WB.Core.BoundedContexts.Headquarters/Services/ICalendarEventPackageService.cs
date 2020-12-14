using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ICalendarEventPackageService
    {
        void ProcessPackage(CalendarEventPackage calendarEventPackage);
    }
}
