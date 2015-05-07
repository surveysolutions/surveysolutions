using System;
using Machine.Specifications;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;

namespace WB.Tests.Unit.Infrastructure.RebuildReadSideCqrsPostgresTransactionManagerTests
{
    internal class when_beginning_command_transaction
    {
        Establish context = () =>
        {
            transactionManager = Create.RebuildReadSideCqrsPostgresTransactionManager();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                transactionManager.BeginCommandTransaction());

        It should_not_fail = () =>
            exception.ShouldBeNull();

        private static RebuildReadSideCqrsPostgresTransactionManager transactionManager;
        private static Exception exception;
    }
}