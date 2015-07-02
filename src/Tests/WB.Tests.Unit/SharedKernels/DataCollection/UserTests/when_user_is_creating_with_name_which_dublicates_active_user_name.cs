using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_user_is_creating_with_name_which_dublicates_active_user_name: UserTestContext
    {
        Establish context = () =>
        {
            SetupInstanceToMockedServiceLocator<IUserPreconditionsService>(
               Mock.Of<IUserPreconditionsService>(_ => _.IsUserNameTakenByActiveUsers(userName) == true));
            eventContext = Create.EventContext();
        };

        Because of = () =>
            exception = Catch.Only<UserException>(() => new User(userId, userName, "", "", new UserRoles[0], false, false, null, "", ""));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;

            SetupInstanceToMockedServiceLocator<IUserPreconditionsService>(
                Mock.Of<IUserPreconditionsService>());
        };

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_UserNameTakenByActiveUsers = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.UserNameTakenByActiveUsers);

        private static EventContext eventContext;
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static string userName = "name";
        private static UserException exception;
    }
}