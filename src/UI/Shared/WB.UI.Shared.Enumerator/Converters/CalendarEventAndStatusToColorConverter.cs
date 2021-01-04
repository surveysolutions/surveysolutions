using System;
using System.Globalization;
using MvvmCross.Converters;
using NodaTime;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class CalendarEventToColorConverter: MvxValueConverter<ZonedDateTime?, int>
    {
        protected override int Convert(ZonedDateTime? calendarEventStart, Type targetType, object parameter, CultureInfo culture)
        {
            if (!calendarEventStart.HasValue)
                return Resource.Color.dashboard_synchronization_regular_title;
            
            var utc = DateTime.UtcNow;
            return utc > calendarEventStart.Value.ToDateTimeUtc()
                ? Resource.Color.dashboard_rejected_tab
                : Resource.Color.dashboard_synchronization_regular_title;
        }
    }
}