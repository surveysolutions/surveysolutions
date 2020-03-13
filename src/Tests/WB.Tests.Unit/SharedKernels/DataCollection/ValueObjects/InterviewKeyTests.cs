using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Unit.SharedKernels.DataCollection.ValueObjects
{
    [TestOf(typeof(InterviewKey))]
    public class InterviewKeyTests
    {
        [Test]
        public void should_format_human_id()
        {
            var key = new InterviewKey(00_00_31_11);

            string formatted = key.ToString();

            Assert.That(formatted, Is.EqualTo("00-00-31-11"));
        }

        [Test]
        public void should_format_large_human_id()
        {
            var key = new InterviewKey(1_00_00_31_11);

            string formatted = key.ToString();

            Assert.That(formatted, Is.EqualTo("01-00-00-31-11"));
        }
    }
}
