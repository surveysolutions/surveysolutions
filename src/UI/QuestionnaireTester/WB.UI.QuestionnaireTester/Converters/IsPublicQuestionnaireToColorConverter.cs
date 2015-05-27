using System;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.CrossCore.UI;
using Cirrious.MvvmCross.Plugins.Color;
using Microsoft.Practices.ServiceLocation;

namespace WB.UI.QuestionnaireTester.Converters
{
    public class IsPublicQuestionnaireToColorConverter : MvxColorValueConverter<bool>
    {
        private IMvxAndroidCurrentTopActivity CurrentTopActivity
        {
            get { return ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>(); }
        }

        protected override MvxColor Convert(bool value, Object parameter, System.Globalization.CultureInfo culture)
        {
            return new MvxColor(this.CurrentTopActivity.Activity.Resources.GetColor(value
                ? Resource.Color.dashboard_public_questionnaire
                : Resource.Color.dashboard_my_questionnaire));
        }
    }
}