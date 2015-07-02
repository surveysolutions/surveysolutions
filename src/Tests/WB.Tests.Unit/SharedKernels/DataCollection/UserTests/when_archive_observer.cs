using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_archive_observer : UserTestContext
    {
        Establish context = () =>
        {
            user = CreateUser();
            user.ApplyEvent(Create.NewUserCreated(role: UserRoles.Observer));
        };

        Because of = () =>
            exception = Catch.Only<UserException>(() => user.Archive());

        Cleanup stuff = () =>
        {
        };

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_RoleDoesntSupportDelete = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.RoleDoesntSupportDelete);

        private static User user;
        private static UserException exception;
    }
}