using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_arhiving_observer : UserTestContext
    {
        Establish context = () =>
        {
            user = CreateUser();
            user.ApplyEvent(Create.NewUserCreated(role: UserRoles.Observer));
        };

        Because of = () =>
            exception = Catch.Only<UserException>(() => user.Archive());

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_RoleDoesntSupportDelete = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.RoleDoesntSupportDelete);

        private static User user;
        private static UserException exception;
    }
}