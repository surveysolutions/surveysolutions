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
        public void Combine_When_Guid_is_combined_with_long_Then_passet_guid_doest_equal_to_result_guid()
        {
            //arrange

            Guid guidValue = Guid.NewGuid();
            long longValue = 5;

            //act

            var result = guidValue.Combine(longValue);

            //assert
            Assert.True(result != guidValue);
        }
    }
}
