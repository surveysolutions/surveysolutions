using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.CrossCore.Converters;
using Cirrious.MvvmCross.Binding.Bindings.SourceSteps;
using Cirrious.MvvmCross.Binding.Combiners;
using Cirrious.MvvmCross.Binding.ExtensionMethods;
using WB.UI.Tester.Converters;

namespace WB.UI.Tester.ValueCombiners
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