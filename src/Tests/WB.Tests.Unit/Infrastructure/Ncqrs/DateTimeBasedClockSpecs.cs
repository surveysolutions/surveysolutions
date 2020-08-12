using System;
using NUnit.Framework;

namespace Ncqrs.Tests
{
    [TestFixture]
    internal class DateTimeBasedClockSpecs
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void When_getting_the_current_time_it_should_be_a_utc_kind()
        {
            var clock = new SystemClock();
            var currentTime = clock.UtcNow();

            Assert.That(currentTime.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void When_getting_the_current_time_it_should_be_the_same_as_the_result_from_the_DateTime_class()
        {
            var clock = new SystemClock();
            
            var currentClockTime = clock.UtcNow();
            var currentDateTimeTime = DateTime.UtcNow;

            Assert.That(currentClockTime, Is.EqualTo(currentDateTimeTime).Within(TimeSpan.FromSeconds(5)));
        }
    }
}
