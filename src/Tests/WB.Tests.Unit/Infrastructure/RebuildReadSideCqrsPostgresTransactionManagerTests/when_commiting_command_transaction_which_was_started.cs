using System;
using Machine.Specifications;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Tests.Unit.Infrastructure.RebuildReadSideCqrsPostgresTransactionManagerTests
{
    internal class when_commiting_command_transaction_which_was_started
    {
        Establish context = () =>
        {
            transactionManager = Create.Other.RebuildReadSideCqrsPostgresTransactionManager();
            transactionManager.BeginCommandTransaction();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                transactionManager.CommitCommandTransaction());

        It should_not_fail = () =>
            exception.ShouldBeNull();

        private static RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions transactionManager;
        private static Exception exception;
    }
}