using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Utility;
using NUnit.Framework;

namespace Main.Core.Tests
{
    [TestFixture]
    public class SimpleObjectExtensionsTests
    {
        [Test]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        public void Combine_When_Guid_is_combined_with_long_Then_passet_guid_doest_equal_to_result_guid(long value)
        {
            //arrange

            Guid guidValue = Guid.NewGuid();
            long longValue = value;

            //act

            var result = guidValue.Combine(longValue);

            //assert
            Assert.AreNotEqual(result.ToString(), guidValue.ToString());
        }
    }
}
