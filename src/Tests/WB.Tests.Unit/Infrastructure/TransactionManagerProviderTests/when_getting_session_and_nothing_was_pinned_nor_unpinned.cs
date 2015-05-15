using Machine.Specifications;
using Moq;
using NHibernate;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.TransactionManagerProviderTests
{
    internal class when_getting_session_and_nothing_was_pinned_nor_unpinned
    {
        Establish context = () =>
        {
            transactionManagerProvider = Create.TransactionManagerProvider(transactionManagerFactory: () => transactionManagerFromFactory);
        };

        Because of = () =>
            session = transactionManagerProvider.GetSession();

        It should_return_session_from_transaction_manager_from_factory = () =>
            session.ShouldEqual(sessionFromTransactionManagerFromFactory);

        private static TransactionManagerProvider transactionManagerProvider;
        private static ISession session;
        private static ISession sessionFromTransactionManagerFromFactory = Mock.Of<ISession>();
        private static ICqrsPostgresTransactionManager transactionManagerFromFactory = Mock.Of<ICqrsPostgresTransactionManager>(_ => _.GetSession() == sessionFromTransactionManagerFromFactory);
    }
}