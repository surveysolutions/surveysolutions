using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Android.Provider;
using Cirrious.CrossCore.Converters;
using Cirrious.MvvmCross.Binding.Bindings.SourceSteps;
using Cirrious.MvvmCross.Binding.Combiners;
using Cirrious.MvvmCross.Binding.ExtensionMethods;
using WB.UI.QuestionnaireTester.Converters;
using WB.UI.QuestionnaireTester.CustomBindings;

namespace WB.UI.QuestionnaireTester.ValueCombiners
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