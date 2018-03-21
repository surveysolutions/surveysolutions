using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection.V4.CustomFunctions;

namespace WB.Tests.Unit.SharedKernels.DataCollection.CustomFunctions
{
    internal class when_generating_random_double 
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            Id1 = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Id2 = Guid.NewGuid();
            Random1 = Id1.GetRandomDouble();
            BecauseOf();
        }

        public void BecauseOf() =>
            Random2 = Id1.GetRandomDouble();

        [NUnit.Framework.Test] public void should_be_equal () =>
            Random2.Should().Be(Random1);


        [NUnit.Framework.Test] public void should_be_equal_for_random () =>
            Id2.GetRandomDouble().Should().Be(Id2.GetRandomDouble());


        private static Guid Id1;
        private static Guid Id2;
        private static double Random1;
        private static double Random2;

    }
}
