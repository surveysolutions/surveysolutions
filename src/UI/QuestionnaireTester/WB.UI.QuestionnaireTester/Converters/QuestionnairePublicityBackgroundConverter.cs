using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;

namespace WB.UI.QuestionnaireTester.Converters
{
    public class QuestionnairePublicityBackgroundConverter : MvxValueConverter<bool, int>
    {
        protected override int Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value)
            {
                return Resource.Drawable.dashboard_my_questionnaires_bg;
            }
            else
            {
                return Resource.Drawable.dashboard_public_questionnaires_bg;
            }
        }
    }
}