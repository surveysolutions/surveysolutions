using System;
using System.Globalization;
using MvvmCross.Converters;
using WB.Core.BoundedContexts.Tester.ViewModels;

namespace WB.UI.Tester.Converters
{
    public class QuestionnaireTypeToBackgroundConverter : MvxValueConverter<QuestionnairesType, int>
    {
        protected override int Convert(QuestionnairesType value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case QuestionnairesType.My:
                    return Resource.Drawable.dashboard_my_questionnaires_bg;
                case QuestionnairesType.SharedWithMe:
                    return Resource.Drawable.dashboard_shared_with_me_questionnaires_bg;
                case QuestionnairesType.Public:
                    return Resource.Drawable.dashboard_public_questionnaires_bg;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
                
        }
    }
}
