using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MvvmCross.Converters;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class TextWithHighlightedSearchTermConverter : MvxValueConverter<string, string>
    {
        protected override string Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value)) return value;

            var searchTerm = (string) parameter;
            if (string.IsNullOrEmpty(searchTerm)) return value;

            var allIndexesOfSearchTerms = value.AllIndexesOf(searchTerm).ToArray();
            if (!allIndexesOfSearchTerms.Any()) return value;

            var valueWithHighlightedSearchTerms = string.Empty;
            var prevIndexOfSearchTermEntry = -1;

            foreach (var indexOfSearchTermEntry in allIndexesOfSearchTerms)
            {
                var substringToHighlight = value.Substring(indexOfSearchTermEntry, searchTerm.Length);

                if (prevIndexOfSearchTermEntry == -1 && indexOfSearchTermEntry > 0)
                    valueWithHighlightedSearchTerms += value.Substring(0, indexOfSearchTermEntry);
                else if(prevIndexOfSearchTermEntry > -1 && prevIndexOfSearchTermEntry + searchTerm.Length < indexOfSearchTermEntry)
                    valueWithHighlightedSearchTerms += value.Substring(prevIndexOfSearchTermEntry + searchTerm.Length, indexOfSearchTermEntry + 1);

                valueWithHighlightedSearchTerms += $"<b>{substringToHighlight}</b>";

                prevIndexOfSearchTermEntry = indexOfSearchTermEntry;
            }

            if(prevIndexOfSearchTermEntry + searchTerm.Length < value.Length)
                valueWithHighlightedSearchTerms += value.Substring(prevIndexOfSearchTermEntry + searchTerm.Length, value.Length - prevIndexOfSearchTermEntry - searchTerm.Length);

            return valueWithHighlightedSearchTerms;
        }
    }
}
