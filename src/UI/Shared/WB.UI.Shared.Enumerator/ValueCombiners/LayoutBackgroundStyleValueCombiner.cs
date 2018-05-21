using System.Collections.Generic;
using MvvmCross.Binding.Extensions;
using WB.UI.Shared.Enumerator.Converters;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class LayoutBackgroundStyleValueCombiner : BaseValueCombiner<QuestionStateStyle>
    {
        protected override int ExpectedParamsCount => 3;

        protected override QuestionStateStyle GetValue(List<object> values)
        {
            bool isInvalid = values[0].ConvertToBoolean();
            bool isAnswered = values[1].ConvertToBoolean();
            bool isDisabled = values[2].ConvertToBoolean();
            
            if (isInvalid)
                return isDisabled ? QuestionStateStyle.InvalidDisabled : QuestionStateStyle.InvalidEnabled;

            if (isAnswered)
                return isDisabled ? QuestionStateStyle.AnsweredDisabled : QuestionStateStyle.AnsweredEnabled;

            if (isDisabled)
                return QuestionStateStyle.NonAnsweredDisabled;

            return QuestionStateStyle.NonAnsweredEnabled;
        }
    }
}
