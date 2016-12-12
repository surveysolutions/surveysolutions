using System;
using Machine.Specifications;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Integration.UtilsTest
{
    [Subject(typeof(Util))]
    [TestOf(typeof(Util))]
    public class UtilTests
    {
        [Test]
        public void When_getting_RosterStringKey_keys_Should_be_same_independent_of_decimal_format()
        {
            //arrange
            Guid itemKey = Guid.NewGuid();
            Identity[] scopeIdsInShourtFormat = new Identity[] { new Identity(itemKey, new RosterVector(new decimal[] {0})) };
            Identity[] scopeIdsInLongFormat = new Identity[] { new Identity(itemKey, new RosterVector(new decimal[] { 0.0m })) };
            
            //act
            var shortKey = Util.GetRosterStringKey(scopeIdsInShourtFormat);
            var longKey = Util.GetRosterStringKey(scopeIdsInLongFormat);

            //assert
            Assert.That(shortKey, Is.EqualTo(longKey));
        }
    }
}
