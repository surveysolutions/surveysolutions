using FluentAssertions;
using Moq;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Infrastructure.TransactionManagerProviderTests
{
    internal class when_getting_transaction_manager_and_nothing_was_pinned_nor_unpinned
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            transactionManagerProvider = Create.Service.TransactionManagerProvider(transactionManagerFactory: () => transactionManagerFromFactory);
            BecauseOf();
        }

        public void BecauseOf() =>
            transactionManager = transactionManagerProvider.GetTransactionManager();

        [NUnit.Framework.Test] public void should_return_transaction_manager_from_factory () =>
            transactionManager.Should().Be(transactionManagerFromFactory);

        private static TransactionManagerProvider transactionManagerProvider;
        private static ITransactionManager transactionManager;
        private static ICqrsPostgresTransactionManager transactionManagerFromFactory = Mock.Of<ICqrsPostgresTransactionManager>();
    }
}
