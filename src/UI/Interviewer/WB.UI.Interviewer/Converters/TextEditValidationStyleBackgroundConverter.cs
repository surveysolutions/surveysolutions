using System;
using System.Globalization;
using MvvmCross.Converters;

namespace WB.UI.Interviewer.Converters
{
    public class TextEditValidationStyleBackgroundConverter : MvxValueConverter<bool, int>
    {
        protected override int Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value)
            {
                return Resource.Drawable.textedit_valid;
            }
            return Resource.Drawable.textedit_invalid;
        }
    }
}