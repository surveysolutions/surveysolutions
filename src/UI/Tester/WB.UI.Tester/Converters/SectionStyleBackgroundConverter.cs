using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;

namespace WB.UI.QuestionnaireTester.Converters
{
    public class SectionStyleBackgroundConverter : MvxValueConverter<bool, int>
    {
        protected override int Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return value
                ? Resource.Drawable.interview_section_background_selected
                : Resource.Drawable.interview_section_background;
        }
    }
}