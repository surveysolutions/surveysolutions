using System;
using System.Globalization;
using MvvmCross.Converters;

namespace WB.UI.Supervisor.MvvmBindings
{
    public class SelectedBackgroundConverter : MvxValueConverter<bool, int>
    {
        protected override int Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
           if (value)
               return Resource.Drawable.question_background_nonanswered;
            return Shared.Enumerator.Resource.Drawable.question_background_nonanswered;
        }
    }
}
