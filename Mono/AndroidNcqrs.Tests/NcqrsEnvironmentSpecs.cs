using System;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Config;
using NUnit.Framework;
using Moq;

namespace Ncqrs.Tests
{
    [TestFixture]
    public class NcqrsEnvironmentSpecs
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        public interface IFoo
        {}

        public class Foo : IFoo
        {}

        public interface IBar
        {}

        [TearDown]
        public void TearDown()
        {
            NcqrsEnvironment.Deconfigure();            
        }

        [Test]
        public void When_get_is_called_when_the_environmemt_is_not_configured_defaults_should_still_be_returned()
        {
            NcqrsEnvironment.Deconfigure();

            var defaultClock = new DateTimeBasedClock();
            NcqrsEnvironment.SetDefault<IClock>(defaultClock);

            NcqrsEnvironment.Get<IClock>().Should().Be(defaultClock);
        }

		[Test] 
		public void Configured_instance_should_over_rule_default()
		{
		    var defaultClock = new DateTimeBasedClock();
		    var configuredClock = new Mock<IClock>().Object;
			IClock ingore = configuredClock;

		    var configuration = new Mock<IEnvironmentConfiguration>();
		    configuration.Setup(m => m.TryGet(out ingore)).Returns(true);

		    NcqrsEnvironment.SetDefault<IClock>(defaultClock);
		    NcqrsEnvironment.Configure(configuration.Object);

		    var result = NcqrsEnvironment.Get<IClock>();

		    Assert.True(configuredClock == result);
		    Assert.False(defaultClock == result);

		    NcqrsEnvironment.Deconfigure();
		}

        [Test] 
        public void Removing_a_default_while_there_is_no_default_registered_should_not_throw_an_exception()
        {
            NcqrsEnvironment.RemoveDefault<IFoo>();
            NcqrsEnvironment.RemoveDefault<IFoo>();
        }

        [Test] 
        public void Setting_a_default_should_multiple_times_should_not_throw_an_exception()
        {
            var defaultFoo = new Mock<IFoo>().Object;
            var newDefaultFoo = new Mock<IFoo>().Object;

            NcqrsEnvironment.SetDefault<IFoo>(defaultFoo);
            NcqrsEnvironment.SetDefault<IFoo>(newDefaultFoo);
            NcqrsEnvironment.SetDefault<IFoo>(defaultFoo);
            NcqrsEnvironment.SetDefault<IFoo>(newDefaultFoo);
        }

        [Test]
        public void Setting_a_default_should_override_the_exiting_default()
        {
            var defaultFoo = new Mock<IFoo>().Object;
            var newDefaultFoo = new Mock<IFoo>().Object;

            NcqrsEnvironment.SetDefault<IFoo>(defaultFoo);
            NcqrsEnvironment.SetDefault<IFoo>(newDefaultFoo);

            var result = NcqrsEnvironment.Get<IFoo>();

            result.Should().BeSameAs(newDefaultFoo);
        }

		[Test]
		public void When_get_is_called_the_call_should_be_redirected_to_the_configuration()
		{
			NcqrsEnvironment.Deconfigure();

			// Arrange
			IFoo outParameter = new Foo();
			var configuration = new Mock<IEnvironmentConfiguration>();
			configuration.Setup(x => x.TryGet(out outParameter)).Returns(true);
			NcqrsEnvironment.Configure(configuration.Object);

			// Act
			NcqrsEnvironment.Get<IFoo>();

			// Assert
			configuration.Verify(x => x.TryGet(out outParameter));
		}

		[Test]
		public void When_get_is_called_the_call_should_return_what_the_environment_configuration_returned()
		{
			NcqrsEnvironment.Deconfigure();

			// Arrange
			IFoo outParameter = new Foo();

			var configuration = new Mock<IEnvironmentConfiguration>();
			configuration.Setup(x => x.TryGet(out outParameter)).Returns(true);
			NcqrsEnvironment.Configure(configuration.Object);

			// Act
			var result = NcqrsEnvironment.Get<IFoo>();

			// Assert
			result.Should().Be(outParameter);
		}

		[Test]
		public void When_get_is_called_but_the_source_did_not_return_an_intance_an_exception_should_be_thrown()
		{
			NcqrsEnvironment.Deconfigure();

			// Arrange
			var mock = new Mock<IEnvironmentConfiguration>();
			NcqrsEnvironment.Configure(mock.Object);

			// Act
			Action act = () => NcqrsEnvironment.Get<IBar>();

			// Assert
			act.ShouldThrow<InstanceNotFoundInEnvironmentConfigurationException>();
		}
    }
}
