using System;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;

namespace Ncqrs.Tests
{
    [TestFixture]
    public class DateTimeBasedClockSpecs
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void When_getting_the_current_time_it_should_be_a_utc_kind()
        {
            var clock = new DateTimeBasedClock();
            var currentTime = clock.UtcNow();

            currentTime.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Test]
        public void When_getting_the_current_time_it_should_be_the_same_as_the_result_from_the_DateTime_class()
        {
            var clock = new DateTimeBasedClock();
            
            var currentClockTime = clock.UtcNow();
            var currentDateTimeTime = DateTime.UtcNow;

            currentClockTime.Should().Be(currentDateTimeTime);
        }
    }
}
