using System;
using System.Globalization;
using MvvmCross.Converters;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class SynchronizationStatusToTextColorConverter : MvxValueConverter<SynchronizationStatus, int>
    {
        protected override int Convert(SynchronizationStatus status, Type targetType, object parameter, CultureInfo culture)
        {
            switch (status)
            {
                case SynchronizationStatus.Canceled:
                case SynchronizationStatus.Fail:
                    return Resource.Color.dashboard_synchronization_fail_title;
                default:
                    return Resource.Color.dashboard_synchronization_regular_title;
            }
        }
    }
}
