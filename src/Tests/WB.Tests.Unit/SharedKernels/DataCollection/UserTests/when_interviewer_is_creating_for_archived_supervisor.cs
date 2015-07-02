using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_interviewer_is_creating_for_archived_supervisor : UserTestContext
    {
        Establish context = () =>
        {
            SetupInstanceToMockedServiceLocator<IUserPreconditionsService>(
               Mock.Of<IUserPreconditionsService>(_ => _.IsUserActive(Moq.It.IsAny<Guid>()) == false));
            eventContext = Create.EventContext();
        };

        Because of = () =>
            exception = Catch.Only<UserException>(() => new User(userId, userName, "", "", new[] { UserRoles.Operator }, false, false, Create.UserLight(), "", ""));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;

            SetupInstanceToMockedServiceLocator<IUserPreconditionsService>(
                Mock.Of<IUserPreconditionsService>());
        };

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_SupervisorArchived = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.SupervisorArchived);

        private static EventContext eventContext;
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static string userName = "name";
        private static UserException exception;
    }
}