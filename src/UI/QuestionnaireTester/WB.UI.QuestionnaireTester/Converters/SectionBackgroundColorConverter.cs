using System.Globalization;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.CrossCore.UI;
using Cirrious.MvvmCross.Plugins.Color;
using Microsoft.Practices.ServiceLocation;

namespace WB.UI.QuestionnaireTester.Converters
{
    public class SectionBackgroundColorConverter : MvxColorValueConverter<bool>
    {
        private IMvxAndroidCurrentTopActivity CurrentTopActivity
        {
            get { return ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>(); }
        }

        protected override MvxColor Convert(bool value, object parameter, CultureInfo culture)
        {
            return new MvxColor(this.CurrentTopActivity.Activity.Resources.GetColor(value
                ? Resource.Color.sections_list_selected_background_color
                : Resource.Color.sections_list_background_color));
        }
    }
}