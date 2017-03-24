using System;
using Machine.Specifications;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.RebuildReadSideCqrsPostgresTransactionManagerTests
{
    internal class when_rolling_back_command_transaction_which_was_started
    {
        Establish context = () =>
        {
            transactionManager = Create.Service.RebuildReadSideCqrsPostgresTransactionManager();
            transactionManager.BeginCommandTransaction();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                transactionManager.RollbackCommandTransaction());

        It should_not_fail = () =>
            exception.ShouldBeNull();

        private static RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions transactionManager;
        private static Exception exception;
    }
}