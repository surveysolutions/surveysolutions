using System;
using System.Globalization;
using MvvmCross.Converters;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class SynchronizationStatusToDrawableConverter : MvxValueConverter<SynchronizationStatus, int>
    {
        protected override int Convert(SynchronizationStatus status, Type targetType, object parameter, CultureInfo culture)
        {
            switch (status)
            {
                case SynchronizationStatus.Download:
                    return Resource.Drawable.synchronization_download_indicator;
                case SynchronizationStatus.Upload:
                    return Resource.Drawable.synchronization_upload_indicator;
                case SynchronizationStatus.Canceled:
                case SynchronizationStatus.Fail:
                    return Resource.Drawable.synchronization_fail_process_indicator;
                case SynchronizationStatus.Success:
                    return Resource.Drawable.synchronization_success_process_indicator;
                default:
                    return Resource.Drawable.synchronization_download_indicator;
            }
        }
    }
}
