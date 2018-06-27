using System;
using System.Globalization;
using MvvmCross.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class TextEditValidationStyleBackgroundConverter : MvxValueConverter<bool, int>
    {
        protected override int Convert(bool value, Type targetType, object parameter, CultureInfo culture) 
            => value ? Resource.Drawable.textedit_valid : Resource.Drawable.textedit_invalid;
    }
}
