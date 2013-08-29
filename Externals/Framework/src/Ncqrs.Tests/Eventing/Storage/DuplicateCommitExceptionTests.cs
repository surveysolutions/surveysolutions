using System;
using FluentAssertions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;

namespace Ncqrs.Tests.Eventing.Storage
{
    [TestFixture]
    public class DuplicateCommitExceptionTests : BaseExceptionTests<DuplicateCommitException>
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        protected override DuplicateCommitException Create()
        {
            return new DuplicateCommitException(Guid.NewGuid(), Guid.NewGuid());
        }

        protected override void VerifyDeserialized(DuplicateCommitException created, DuplicateCommitException deserialized)
        {
            deserialized.EventSourceId.Should().Be(created.EventSourceId);
            deserialized.CommitId.Should().Be(created.CommitId);
        }
    }
}