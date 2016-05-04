using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Binding.Bindings.SourceSteps;
using MvvmCross.Binding.Combiners;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Platform.Converters;
using WB.UI.Shared.Enumerator.Converters;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class LayoutBackgroundStyleValueCombiner: MvxValueCombiner
    {
        public override Type SourceType(IEnumerable<IMvxSourceStep> steps)
        {
            return typeof(QuestionStateStyle);
        }

        public override bool TryGetValue(IEnumerable<IMvxSourceStep> steps, out Object value)
        {
            var sourceSteps = steps.ToList();

            if (sourceSteps.Count < 2)
                return base.TryGetValue(sourceSteps, out value);

            var isInvalidValue = sourceSteps[0].GetValue();
            var isAnsweredValue = sourceSteps[1].GetValue();

            if (isInvalidValue == MvxBindingConstant.DoNothing
                || isAnsweredValue == MvxBindingConstant.DoNothing)
            {
                value = MvxBindingConstant.DoNothing;
                return false;
            }

            if (isInvalidValue == MvxBindingConstant.UnsetValue
                || isAnsweredValue == MvxBindingConstant.UnsetValue)
            {
                value = MvxBindingConstant.UnsetValue;
                return false;
            }

            var isInvalid = isInvalidValue.ConvertToBoolean();
            var isAnswered = isAnsweredValue.ConvertToBoolean();

            if (isInvalid)
            {
                value = QuestionStateStyle.Invalid;
            }
            else if (isAnswered)
            {
                value = QuestionStateStyle.Answered;
            }
            else
            {
                value = QuestionStateStyle.NonAnswered;
            }

            return true;
        }
    }
}