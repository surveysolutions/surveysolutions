using System;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.GenericSubdomains.Utils.ObjectExtensionsTests
{
    internal class when_wrapping_value_with_initialized_lazy_class
    {
        Because of = () =>
            result = value.ToInitializedLazy();

        It should_return_result_which_value_is_initialized = () =>
            result.IsValueCreated.ShouldBeTrue();

        private static Lazy<object> result;
        private static object value = new object();
    }
}