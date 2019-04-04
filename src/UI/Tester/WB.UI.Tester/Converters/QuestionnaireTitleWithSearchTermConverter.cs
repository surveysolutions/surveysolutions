using System;
using System.Globalization;
using System.Text.RegularExpressions;
using MvvmCross.Converters;

namespace WB.UI.Tester.Converters
{
    public class QuestionnaireTitleWithSearchTermConverter : MvxValueConverter<string, string>
    {
        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            var searchTerm = (string) parameter;

            if (string.IsNullOrEmpty(searchTerm)) return value;

            var index = CultureInfo.CurrentCulture.CompareInfo.IndexOf(value, searchTerm, CompareOptions.IgnoreCase);

            string title;
            if (index >= 0)
            {
                var substringToHighlight = value.Substring(index, searchTerm.Length);
                title = Regex.Replace(value, Regex.Escape(searchTerm), "<b>" + substringToHighlight + "</b>", RegexOptions.IgnoreCase);
            }
            else
            {
                title = value;
            }
            return title;
        }
    }
}