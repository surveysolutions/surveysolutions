using System.Collections.Generic;

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
                result += $" - <i>{rosterTitle}</i>";
            }
             
            return result;
        }
    }
}