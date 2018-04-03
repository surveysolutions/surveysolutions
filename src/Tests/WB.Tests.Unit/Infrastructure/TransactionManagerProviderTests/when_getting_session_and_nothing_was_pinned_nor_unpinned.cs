using FluentAssertions;
using Moq;
using NHibernate;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Infrastructure.TransactionManagerProviderTests
{
    internal class when_getting_session_and_nothing_was_pinned_nor_unpinned
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            transactionManagerProvider = Create.Service.TransactionManagerProvider(transactionManagerFactory: () => transactionManagerFromFactory);
            BecauseOf();
        }

        public void BecauseOf() =>
            session = transactionManagerProvider.GetSession();

        [NUnit.Framework.Test] public void should_return_session_from_transaction_manager_from_factory () =>
            session.Should().Be(sessionFromTransactionManagerFromFactory);

        private static TransactionManagerProvider transactionManagerProvider;
        private static ISession session;
        private static ISession sessionFromTransactionManagerFromFactory = Mock.Of<ISession>();
        private static ICqrsPostgresTransactionManager transactionManagerFromFactory = Mock.Of<ICqrsPostgresTransactionManager>(_ => _.GetSession() == sessionFromTransactionManagerFromFactory);
    }
}
