using Machine.Specifications;
using Moq;
using NHibernate;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.TransactionManagerProviderTests
{
    internal class when_getting_session_and_rebuild_read_side_transaction_manager_was_pinned
    {
        Establish context = () =>
        {
            transactionManagerProvider = Create.TransactionManagerProvider(rebuildReadSideTransactionManager: rebuildReadSideTransactionManager);
            transactionManagerProvider.PinRebuildReadSideTransactionManager();
        };

        Because of = () =>
            session = transactionManagerProvider.GetSession();

        It should_return_session_from_rebuild_read_side_transaction_manager = () =>
            session.ShouldEqual(sessionFromRebuildReadSideTransactionManager);

        private static TransactionManagerProvider transactionManagerProvider;
        private static ISession session;
        private static ISession sessionFromRebuildReadSideTransactionManager = Mock.Of<ISession>();
        private static ICqrsPostgresTransactionManager rebuildReadSideTransactionManager = Mock.Of<ICqrsPostgresTransactionManager>(_ => _.GetSession() == sessionFromRebuildReadSideTransactionManager);
    }
}