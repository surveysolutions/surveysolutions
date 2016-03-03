using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Binding.Bindings.SourceSteps;
using MvvmCross.Binding.Combiners;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Platform.Converters;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class NotValueCombiner : MvxValueCombiner
    {
        public override Type SourceType(IEnumerable<IMvxSourceStep> steps)
        {
            return typeof(bool);
        }

        public override bool TryGetValue(IEnumerable<IMvxSourceStep> steps, out Object value)
        {
            var sourceSteps = steps.ToList();

            if (sourceSteps.Count < 2)
                return base.TryGetValue(sourceSteps, out value);

            object providedValue = sourceSteps[0].GetValue();

            if (providedValue == MvxBindingConstant.DoNothing)
            {
                value = MvxBindingConstant.DoNothing;
                return false;
            }

            if (providedValue == MvxBindingConstant.UnsetValue)
            {
                value = MvxBindingConstant.UnsetValue;
                return false;
            }

            var providedBoolValue = providedValue.ConvertToBoolean();

            value = !providedBoolValue;

            return true;
        }
    }
}