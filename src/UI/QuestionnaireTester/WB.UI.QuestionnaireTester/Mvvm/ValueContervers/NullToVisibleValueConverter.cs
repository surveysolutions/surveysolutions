using Cirrious.MvvmCross.Plugins.Visibility;

namespace WB.UI.QuestionnaireTester.Mvvm.ValueContervers
{
    public class NullToVisibleValueConverter : MvxVisibilityValueConverter
    {
        protected override bool IsATrueValue(object value, object parameter, bool defaultValue)
        {
            return value == null;
        }
    }
}
