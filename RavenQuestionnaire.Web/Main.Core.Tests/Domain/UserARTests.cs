namespace Main.Core.Tests.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Domain;
    using Main.Core.Events.User;

    using NUnit.Framework;

    using Ncqrs.Spec;

    [TestFixture]
    public class UserARTests
    {
        private EventContext eventContext;

        [SetUp]
        public void Init()
        {
            this.eventContext = new EventContext();
        }

        [TearDown]
        public void Dispose()
        {
            this.eventContext.Dispose();
            this.eventContext = null;
        }

        [Test]
        public void Lock_When_called_Then_raised_UserLocked_event()
        {
            // arrange
            UserAR user = this.CreateUserAR();

            // act
            user.Lock();

            // assert
            Assert.That(this.GetRaisedEvents<UserLocked>().Count(), Is.EqualTo(1));
        }

        private UserAR CreateUserAR()
        {
            return new UserAR();
        }

        private IEnumerable<T> GetRaisedEvents<T>()
        {
            return this.eventContext
                .Events
                .Where(e => e.Payload is T)
                .Select(e => e.Payload)
                .Cast<T>();
        }
    }
}