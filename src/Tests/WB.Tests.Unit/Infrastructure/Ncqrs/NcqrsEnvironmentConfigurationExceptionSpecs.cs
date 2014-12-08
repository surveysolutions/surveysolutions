﻿using System;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using System.IO;

namespace Ncqrs.Tests
{
    [TestFixture]
    public class NcqrsEnvironmentConfigurationExceptionSpecs
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_message()
        {
            String message = "Hello world";

            var target = new NcqrsEnvironmentException(message);

            target.Message.Should().Be(message);
        }

        [Test]
        public void Constructing_an_instance_should_initialize_the_inner_exception()
        {
            String aMessage = "Hello world";
            var theInnerException = new Exception();

            var target = new NcqrsEnvironmentException(aMessage, theInnerException);

            target.InnerException.Should().Be(theInnerException);
        }
    }
}
