using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_archive_interviewer_with_assigments : UserTestContext
    {
        Establish context = () =>
        {
            SetupInstanceToMockedServiceLocator<IUserPreconditionsService>(
             Mock.Of<IUserPreconditionsService>(_ => _.CountOfInterviewsInterviewerResposibleFor(Moq.It.IsAny<Guid>()) == 1));

            user = CreateUser();
            user.ApplyEvent(Create.NewUserCreated());
        };

        Because of = () =>
            exception = Catch.Only<UserException>(() => user.Archive());

        Cleanup stuff = () =>
        {
            SetupInstanceToMockedServiceLocator<IUserPreconditionsService>(
              Mock.Of<IUserPreconditionsService>());
        };

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_UserHasAssigments = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.UserHasAssigments);

        private static User user;
        private static UserException exception;
         
    }
}