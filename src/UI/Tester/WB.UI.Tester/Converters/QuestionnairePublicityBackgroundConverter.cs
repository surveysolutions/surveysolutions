using System;
using System.Globalization;
using MvvmCross.Converters;

namespace WB.UI.Tester.Converters
{
    public class QuestionnairePublicityBackgroundConverter : MvxValueConverter<bool, int>
    {
        protected override int Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ? Resource.Drawable.dashboard_public_questionnaires_bg : Resource.Drawable.dashboard_my_questionnaires_bg;
        }
    }
}
