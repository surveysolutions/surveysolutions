using System.Collections.Generic;
using MvvmCross.Binding.Extensions;
using WB.UI.Shared.Enumerator.Converters;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class LayoutOptionBackgroundStyleValueCombiner : LayoutBackgroundStyleValueCombiner
    {
        protected override int ExpectedParamsCount => 5;

        protected override QuestionStateStyle GetValue(List<object> values)
        {
            bool isProtected = values[3].ConvertToBoolean();
            bool canBeChecked = values[4].ConvertToBoolean();

            if (!canBeChecked)
                return QuestionStateStyle.MaxAnswersCountReached;

            if (isProtected)
                return QuestionStateStyle.AnsweredProtected;

            return base.GetValue(values);
        }
    }
}
