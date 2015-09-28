using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
using WB.Core.BoundedContexts.Interviewer.Views;


namespace WB.UI.Interviewer.Converters
{
    public class IsSynchronizationFailConverter : MvxValueConverter<SynchronizationStatus, bool>
    {
        protected override bool Convert(SynchronizationStatus status, Type targetType, object parameter, CultureInfo culture)
        {
            return status == SynchronizationStatus.Fail;
        }
    }
}