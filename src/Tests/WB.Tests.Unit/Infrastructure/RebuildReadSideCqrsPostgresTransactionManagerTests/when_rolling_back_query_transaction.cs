using System;
using Machine.Specifications;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;

namespace WB.Tests.Unit.Infrastructure.RebuildReadSideCqrsPostgresTransactionManagerTests
{
    internal class when_rolling_back_query_transaction
    {
        Establish context = () =>
        {
            transactionManager = Create.RebuildReadSideCqrsPostgresTransactionManager();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                transactionManager.RollbackQueryTransaction());

        It should_throw_NotSupportedException = () =>
            exception.ShouldBeOfExactType<NotSupportedException>();

        private static RebuildReadSideCqrsPostgresTransactionManager transactionManager;
        private static Exception exception;
    }
}