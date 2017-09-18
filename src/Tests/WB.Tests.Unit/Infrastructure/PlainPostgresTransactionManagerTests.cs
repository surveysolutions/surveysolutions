using Moq;
using NHibernate;
using NUnit.Framework;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure
{
    [TestFixture]
    [TestOf(typeof(PlainPostgresTransactionManager))]
    internal class PlainPostgresTransactionManagerTests
    {
        [Test]
        public void When_starting_transaction_Then_does_not_open_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory>();
            var transactionManager = Create.Service.PlainPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            // act
            transactionManager.BeginTransaction();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Never);
        }

        [Test]
        public void When_getting_session_first_time_after_transaction_start_Then_opens_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory> { DefaultValue = DefaultValue.Mock };
            var transactionManager = Create.Service.PlainPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            transactionManager.BeginTransaction();

            sessionFactoryMock.ResetCalls();

            // act
            transactionManager.GetSession();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Once);
        }

        [Test]
        public void When_getting_session_second_time_after_transaction_start_Then_does_not_open_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory> { DefaultValue = DefaultValue.Mock };
            var transactionManager = Create.Service.PlainPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            transactionManager.BeginTransaction();
            transactionManager.GetSession();

            sessionFactoryMock.ResetCalls();

            // act
            transactionManager.GetSession();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Never);
        }

        [Test]
        public void When_rolling_back_started_transaction_without_getting_session_Then_does_not_open_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory> { DefaultValue = DefaultValue.Mock };
            var transactionManager = Create.Service.PlainPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            // act
            transactionManager.BeginTransaction();
            transactionManager.RollbackTransaction();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Never);
        }

        [Test]
        public void When_commiting_started_transaction_without_getting_session_Then_does_not_open_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory> { DefaultValue = DefaultValue.Mock };
            var transactionManager = Create.Service.PlainPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            // act
            transactionManager.BeginTransaction();
            transactionManager.CommitTransaction();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Never);
        }
    }
}