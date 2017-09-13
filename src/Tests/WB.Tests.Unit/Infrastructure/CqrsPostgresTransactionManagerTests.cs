using Moq;
using NHibernate;
using NUnit.Framework;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure
{
    [TestFixture]
    [TestOf(typeof(CqrsPostgresTransactionManager))]
    internal class CqrsPostgresTransactionManagerTests
    {
        [Test]
        public void When_starting_command_transaction_Then_does_not_open_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory>();
            var transactionManager = Create.Service.CqrsPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            // act
            transactionManager.BeginCommandTransaction();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Never);
        }

        [Test]
        public void When_getting_session_first_time_after_command_transaction_start_Then_opens_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory> { DefaultValue = DefaultValue.Mock };
            var transactionManager = Create.Service.CqrsPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            transactionManager.BeginCommandTransaction();

            sessionFactoryMock.ResetCalls();

            // act
            transactionManager.GetSession();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Once);
        }

        [Test]
        public void When_getting_session_second_time_after_command_transaction_start_Then_does_not_open_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory> { DefaultValue = DefaultValue.Mock };
            var transactionManager = Create.Service.CqrsPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            transactionManager.BeginCommandTransaction();
            transactionManager.GetSession();

            sessionFactoryMock.ResetCalls();

            // act
            transactionManager.GetSession();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Never);
        }

        [Test]
        public void When_rolling_back_started_command_transaction_without_getting_session_Then_does_not_open_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory>();
            var transactionManager = Create.Service.CqrsPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            // act
            transactionManager.BeginCommandTransaction();
            transactionManager.RollbackCommandTransaction();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Never);
        }

        [Test]
        public void When_commiting_back_started_command_transaction_without_getting_session_Then_does_not_open_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory>();
            var transactionManager = Create.Service.CqrsPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            // act
            transactionManager.BeginCommandTransaction();
            transactionManager.CommitCommandTransaction();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Never);
        }

        [Test]
        public void When_starting_query_transaction_Then_does_not_open_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory>();
            var transactionManager = Create.Service.CqrsPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            // act
            transactionManager.BeginCommandTransaction();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Never);
        }

        [Test]
        public void When_getting_session_first_time_after_query_transaction_start_Then_opens_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory> { DefaultValue = DefaultValue.Mock };
            var transactionManager = Create.Service.CqrsPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            transactionManager.BeginCommandTransaction();

            sessionFactoryMock.ResetCalls();

            // act
            transactionManager.GetSession();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Once);
        }

        [Test]
        public void When_getting_session_second_time_after_query_transaction_start_Then_does_not_open_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory> { DefaultValue = DefaultValue.Mock };
            var transactionManager = Create.Service.CqrsPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            transactionManager.BeginCommandTransaction();
            transactionManager.GetSession();

            sessionFactoryMock.ResetCalls();

            // act
            transactionManager.GetSession();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Never);
        }

        [Test]
        public void When_rolling_back_started_query_transaction_without_getting_session_Then_does_not_open_session()
        {
            // arrange
            var sessionFactoryMock = new Mock<ISessionFactory>();
            var transactionManager = Create.Service.CqrsPostgresTransactionManager(sessionFactory: sessionFactoryMock.Object);

            // act
            transactionManager.BeginCommandTransaction();
            transactionManager.RollbackCommandTransaction();

            // assert
            sessionFactoryMock.Verify(factory => factory.OpenSession(), Times.Never);
        }
    }
}