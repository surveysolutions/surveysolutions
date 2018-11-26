using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Integration.UtilsTest
{
    [TestFixture]
    public class MinusDelimitedIntArrayParserTest
    { 
        [TestCase("1-2-3", 1, 2, 3)]
        [TestCase("-1-2-3", -1, 2, 3)]
        [TestCase("-3", -3)]
        [TestCase("1--2--3", 1, -2, -3)]
        [TestCase("1--2--3-", 1, -2, -3)]
        public void should_parse_input_string_into_int_array_result(string input, params int[] result)
        {
            var parse = input.ParseMinusDelimitedIntArray();
            Assert.That(parse, Is.EqualTo(result));
        }
    }
}
