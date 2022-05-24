using System;
using System.Globalization;
using MvvmCross.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class TextWithHighlightedSearchTermConverter : MvxValueConverter<string, string>
    {
        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value)) return value;

            var searchTerm = parameter as string;
            if (string.IsNullOrEmpty(searchTerm)) return value;

            var lastIndex = value.LastIndexOf(searchTerm, StringComparison.OrdinalIgnoreCase);
            while (lastIndex >= 0)
            {
                value = value.Insert(lastIndex + searchTerm.Length, "</b>").Insert(lastIndex, "<b>");
                lastIndex = value.LastIndexOf(searchTerm, lastIndex, StringComparison.OrdinalIgnoreCase);
            }

            return value;
        }
    }
}
