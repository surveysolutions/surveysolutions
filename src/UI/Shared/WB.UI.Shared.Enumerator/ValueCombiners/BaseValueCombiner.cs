using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Binding.Bindings.SourceSteps;
using MvvmCross.Binding.Combiners;
using MvvmCross.Converters;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public abstract class BaseValueCombiner<TResult> : MvxValueCombiner
    {
        protected abstract int ExpectedParamsCount { get; }

        protected abstract TResult GetValue(List<object> values);

        public override Type SourceType(IEnumerable<IMvxSourceStep> steps) => typeof(TResult);

        public override bool TryGetValue(IEnumerable<IMvxSourceStep> steps, out object value)
        {
            var values = steps.Select(step => step.GetValue()).ToList();

            if (values.Count != this.ExpectedParamsCount)
                return base.TryGetValue(steps, out value);

            if (values.Contains(MvxBindingConstant.DoNothing))
            {
                value = MvxBindingConstant.DoNothing;
                return false;
            }

            if (values.Contains(MvxBindingConstant.UnsetValue))
            {
                value = MvxBindingConstant.UnsetValue;
                return false;
            }

            value = this.GetValue(values);
            return true;
        }
    }
}
