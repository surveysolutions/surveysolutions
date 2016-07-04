using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Binding.Bindings.SourceSteps;
using MvvmCross.Binding.Combiners;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Platform.Converters;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class NotValueCombiner : BaseValueCombiner<bool>
    {
        protected override int ExpectedParamsCount => 1;

        protected override bool GetValue(List<object> values) => !values.Single().ConvertToBoolean();
    }
}