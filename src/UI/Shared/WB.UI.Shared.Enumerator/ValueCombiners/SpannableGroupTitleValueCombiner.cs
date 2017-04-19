using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class SpannableGroupTitleValueCombiner : BaseValueCombiner<string>
    {
        protected override int ExpectedParamsCount => 2;

        protected override string GetValue(List<object> values)
        {
            string groupTitle = values[0]?.ToString() ?? string.Empty;
            string rosterTitle = values[1]?.ToString() ?? string.Empty;

            string result = groupTitle;
            if (!string.IsNullOrWhiteSpace(rosterTitle))
            {
                string pattern = Regex.Escape(rosterTitle);
                result = Regex.Replace(groupTitle, @" \- " + pattern, m => $"<i>{m.Value}</i>");
            }
             
            return result;
        }
    }
}