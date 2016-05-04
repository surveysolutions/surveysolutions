using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.TransactionManagerProviderTests
{
    internal class when_getting_transaction_manager_and_rebuild_read_side_transaction_manager_was_pinned_but_then_unpinned
    {
        Establish context = () =>
        {
            transactionManagerProvider = Create.TransactionManagerProvider(transactionManagerFactory: () => transactionManagerFromFactory);
            transactionManagerProvider.PinRebuildReadSideTransactionManager();
            transactionManagerProvider.UnpinTransactionManager();
        };

        Because of = () =>
            transactionManager = transactionManagerProvider.GetTransactionManager();

        It should_return_transaction_manager_from_factory = () =>
            transactionManager.ShouldEqual(transactionManagerFromFactory);

        private static TransactionManagerProvider transactionManagerProvider;
        private static ITransactionManager transactionManager;
        private static ICqrsPostgresTransactionManager transactionManagerFromFactory = Mock.Of<ICqrsPostgresTransactionManager>();
    }
}