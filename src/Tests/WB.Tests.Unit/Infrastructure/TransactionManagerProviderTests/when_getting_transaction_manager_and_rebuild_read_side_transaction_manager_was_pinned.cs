using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.TransactionManagerProviderTests
{
    internal class when_getting_transaction_manager_and_rebuild_read_side_transaction_manager_was_pinned
    {
        Establish context = () =>
        {
            transactionManagerProvider = Create.TransactionManagerProvider(rebuildReadSideTransactionManager: rebuildReadSideTransactionManager);
            transactionManagerProvider.PinRebuildReadSideTransactionManager();
        };

        Because of = () =>
            transactionManager = transactionManagerProvider.GetTransactionManager();

        It should_return_rebuild_read_side_transaction_manager = () =>
            transactionManager.ShouldEqual(rebuildReadSideTransactionManager);

        private static TransactionManagerProvider transactionManagerProvider;
        private static ITransactionManager transactionManager;
        private static ICqrsPostgresTransactionManager rebuildReadSideTransactionManager = Mock.Of<ICqrsPostgresTransactionManager>();
    }
}