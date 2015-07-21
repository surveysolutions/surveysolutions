using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_unarchive_active_user : UserTestContext
    {
        Establish context = () =>
        {
            user = CreateUser();
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