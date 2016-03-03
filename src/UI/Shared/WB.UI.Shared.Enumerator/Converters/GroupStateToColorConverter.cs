﻿using System;
using System.Globalization;
using Microsoft.Practices.ServiceLocation;
using MvvmCross.Platform.Converters;
using MvvmCross.Platform.Droid.Platform;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class GroupStateToColorConverter : MvxValueConverter<SimpleGroupStatus, int>
    {
        protected override int Convert(SimpleGroupStatus value, Type targetType, object parameter, CultureInfo culture)
        {
            var mvxAndroidCurrentTopActivity = ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>();
            var resources = mvxAndroidCurrentTopActivity.Activity.Resources;

            switch (value)
            {
                case SimpleGroupStatus.Completed:
                    return Resource.Color.group_completed;
                case SimpleGroupStatus.Invalid:
                    return Resource.Color.group_with_invalid_answers;
                default:
                    return Resource.Color.group_started;
            }
        }
    }
}