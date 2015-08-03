using System;
using System.Globalization;
using Android.Graphics;
using Cirrious.CrossCore.Converters;
using Cirrious.CrossCore.Droid.Platform;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Tester.ViewModels.Groups;
using WB.UI.Shared.Enumerator;

namespace WB.UI.Tester.Converters
{
    public class GroupStateToColorConverter : MvxValueConverter<SimpleGroupStatus, Color>
    {
        protected override Color Convert(SimpleGroupStatus value, Type targetType, object parameter, CultureInfo culture)
        {
            var mvxAndroidCurrentTopActivity = ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>();
            var resources = mvxAndroidCurrentTopActivity.Activity.Resources;

            switch (value)
            {
                case SimpleGroupStatus.Completed:
                    return resources.GetColor(Resource.Color.group_completed);
                case SimpleGroupStatus.Invalid:
                    return resources.GetColor(Resource.Color.group_with_invalid_answers);
                default:
                    return resources.GetColor(Resource.Color.group_started);
            }
        }
    }
}