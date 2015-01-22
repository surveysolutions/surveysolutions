using System.Collections;
using Cirrious.MvvmCross.Plugins.Visibility;

namespace WB.UI.QuestionnaireTester.ValueContervers
{
    public class VisibleIfListIsEmptyValueConverter : MvxVisibilityValueConverter
    {
        protected override bool IsATrueValue(object value, object parameter, bool defaultValue)
        {
            var collection = value as ICollection;
            return collection == null || (collection.Count == 0);
        }
    }
}
