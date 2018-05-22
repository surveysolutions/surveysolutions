﻿using System.Collections.Generic;
using MvvmCross.Binding.ExtensionMethods;
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

            var state = base.GetValue(values);

            if (state == QuestionStateStyle.InvalidEnabled || state == QuestionStateStyle.InvalidDisabled)
            {
                return state;
            }

            if (isProtected)
            {
                return QuestionStateStyle.AnsweredProtected;
            }

            return state;
        }
    }
}
