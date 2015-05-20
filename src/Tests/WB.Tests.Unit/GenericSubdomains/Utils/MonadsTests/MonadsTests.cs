using Machine.Specifications;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.GenericSubdomains.Utils.MonadsTests
{
    public class MonadsTests
    {
        Establish context = () => {};

        It should_work_correctly_with_methods = () =>
        {
            var testVariable = new ClassWithMethods();

            long x = testVariable.Get(3).X;
            x.ShouldEqual(3);

            x = Monads.Maybe(() => testVariable.Get(3).X);
            x.ShouldEqual(3);

            x = Monads.Maybe(() => testVariable.GetDefault().X);
            x.ShouldEqual(42);

            x = Monads.Maybe(() => testVariable.GetItself().Get(100500).X);
            x.ShouldEqual(100500);

            x = Monads.Maybe(() => testVariable.GetItself().GetNull().X);
            x.ShouldEqual(0);
        };

        It should_work_correctly_with_nullables = () =>
        {
            var testVariable = new ClassWithNestedObject();

            long x = Monads.Maybe(() => testVariable.Field.X);
            x.ShouldEqual(0);

            var nullableX = Monads.Maybe<long?>(() => testVariable.Field.X);
            nullableX.ShouldBeNull();

            testVariable.Field = new NestedClass
            {
                X = 3
            };

            x = Monads.Maybe(() => testVariable.Field.X);
            x.ShouldEqual(3);

            nullableX = Monads.Maybe<long?>(() => testVariable.Field.X);
            nullableX.ShouldEqual(3);
        };

        It should_work_correctly_with_null_coalesce_operator = () => 
        {
            int? nullValue = null;
            int? notNullValue = 3;

            var nullResult = Monads.Maybe(() => nullValue ?? nullValue);
            nullResult.ShouldEqual(nullValue);

            var notNullResultFirst = Monads.Maybe(() => notNullValue ?? nullValue);
            notNullResultFirst.ShouldEqual(notNullValue);

            var notNullResultSecond = Monads.Maybe(() => nullValue ?? notNullValue);
            notNullResultSecond.ShouldEqual(notNullValue);

            int? anotherNotNullValue = 5;

            var anotherNotNullResult = Monads.Maybe(() => anotherNotNullValue ?? notNullValue);
            anotherNotNullResult.ShouldEqual(anotherNotNullValue);
        };

        It should_return_null_for_nullable_types_with_null_value_When_called_method_on_them = () =>
        {
            var testClass = new NestedClass();

            string result = Monads.Maybe(() => testClass.Y.ToString());
            result.ShouldBeNull();
        };

        It should_support_static_methods_wrap = () =>
        {
            var result = Monads.Maybe(() => string.Format("{0}", "42"));

            result.ShouldEqual("42");
        };

        It should_support_cast_expressions = () =>
        {
            var result = Monads.Maybe(() => string.Format("{0}", 42));

            result.ShouldEqual("42");
        };

        private class NestedClass
        {
            public long X { get; set; }

            public long? Y { get; set; }
        }

        private class ClassWithNestedObject
        {
            public NestedClass Field { get; set; }
        }

        private class ClassWithMethods
        {
            public NestedClass Get(long x)
            {
                return new NestedClass
                {
                    X = x
                };
            }

            public NestedClass GetDefault()
            {
                return new NestedClass
                {
                    X = 42
                };
            }

            public ClassWithMethods GetItself()
            {
                return this;
            }

            public NestedClass GetNull()
            {
                return null;
            }
        }
    }

}