using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.V4.CustomFunctions;

namespace WB.Tests.Unit.SharedKernels.DataCollection.CustomFunctions
{
    internal class when_generating_random_double 
    {
        Establish context = () =>
        {
            Id1 = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Id2 = Guid.NewGuid();
            Random1 = Id1.GetRandomDouble();
        };

        Because of = () =>
            Random2 = Id1.GetRandomDouble();

        It should_be_equal = () =>
            Random2.ShouldEqual(Random1);


        It should_be_equal_for_random = () =>
            Id2.GetRandomDouble().ShouldEqual(Id2.GetRandomDouble());


        private static Guid Id1;
        private static Guid Id2;
        private static double Random1;
        private static double Random2;

    }
}
