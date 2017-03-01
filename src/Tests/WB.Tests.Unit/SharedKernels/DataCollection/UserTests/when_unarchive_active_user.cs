using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_unarchive_active_user : UserTestContext
    {
        Establish context = () =>
        {
            var userId = Guid.NewGuid();
            user = Create.Entity.User(userId);
            user.Roles = new UserRoles[] { UserRoles.Supervisor };
            user.IsArchived = false;
        };

        Because of = () =>
            exception = Catch.Only<UserException>(() => user.Unarchive());

        Cleanup stuff = () =>
        {
        };

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_UserIsNotArchived = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.UserIsNotArchived);

        private static User user;
        private static UserException exception;
    }
}