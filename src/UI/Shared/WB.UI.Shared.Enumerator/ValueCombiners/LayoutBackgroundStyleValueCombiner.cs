using System.Collections.Generic;
using MvvmCross.Binding.ExtensionMethods;
using WB.UI.Shared.Enumerator.Converters;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class LayoutBackgroundStyleValueCombiner : BaseValueCombiner<QuestionStateStyle>
    {
        protected override int ExpectedParamsCount => 2;

        protected override QuestionStateStyle GetValue(List<object> values)
        {
            bool isInvalid = values[0].ConvertToBoolean();
            bool isAnswered = values[1].ConvertToBoolean();

            return isInvalid
                ? QuestionStateStyle.Invalid
                : (isAnswered
                    ? QuestionStateStyle.Answered
                    : QuestionStateStyle.NonAnswered);
        }
    }
}