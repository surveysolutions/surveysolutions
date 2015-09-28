﻿using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
using WB.Core.BoundedContexts.Interviewer.Views;


namespace WB.UI.Interviewer.Converters
{
    public class SynchronizationStatusToTextColorConverter : MvxValueConverter<SynchronizationStatus, int>
    {
        protected override int Convert(SynchronizationStatus status, Type targetType, object parameter, CultureInfo culture)
        {
            switch (status)
            {
                case SynchronizationStatus.Fail:
                    return Resource.Color.dashboard_synchronization_fail_title;
                default:
                    return Resource.Color.dashboard_synchronization_regular_title;
            }
        }
    }
}