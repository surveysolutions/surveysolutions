using System;
using Machine.Specifications;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;

namespace WB.Tests.Unit.Infrastructure.RebuildReadSideCqrsPostgresTransactionManagerTests
{
    internal class when_commiting_command_transaction_which_was_not_started
    {
        Establish context = () =>
        {
            transactionManager = Create.RebuildReadSideCqrsPostgresTransactionManager();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                transactionManager.CommitCommandTransaction());

        It should_throw_InvalidOperationException = () =>
            exception.ShouldBeOfExactType<InvalidOperationException>();

        private static RebuildReadSideCqrsPostgresTransactionManager transactionManager;
        private static Exception exception;
    }
}