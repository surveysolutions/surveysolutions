using System.Collections.Generic;
using System.Linq;
using MvvmCross.Binding.Extensions;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class NotValueCombiner : BaseValueCombiner<bool>
    {
        protected override int ExpectedParamsCount => 1;

        protected override bool GetValue(List<object> values) => !values.Single().ConvertToBoolean();
    }
}
