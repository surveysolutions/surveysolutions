using System;
using System.Globalization;
using MvvmCross.Converters;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class GroupStateToColorConverter : MvxValueConverter<SimpleGroupStatus, int>
    {
        protected override int Convert(SimpleGroupStatus value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case SimpleGroupStatus.Completed:
                    return Resource.Color.group_completed;
                case SimpleGroupStatus.Invalid:
                    return Resource.Color.group_with_invalid_answers;
                case SimpleGroupStatus.Disabled:
                    return Resource.Color.group_disabled;
                default:
                    return Resource.Color.group_started;
            }
        }
    }
}
