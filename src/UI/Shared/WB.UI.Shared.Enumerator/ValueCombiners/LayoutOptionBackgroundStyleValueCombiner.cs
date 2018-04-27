using System.Collections.Generic;
using MvvmCross.Binding.ExtensionMethods;
using WB.UI.Shared.Enumerator.Converters;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class LayoutOptionBackgroundStyleValueCombiner : LayoutBackgroundStyleValueCombiner
    {
        protected override int ExpectedParamsCount => 4;

        protected override QuestionStateStyle GetValue(List<object> values)
        {
            bool isProtected = values[3].ConvertToBoolean();

            if (isProtected)
                return QuestionStateStyle.AnsweredProtected;

            return base.GetValue(values);
        }
    }
}