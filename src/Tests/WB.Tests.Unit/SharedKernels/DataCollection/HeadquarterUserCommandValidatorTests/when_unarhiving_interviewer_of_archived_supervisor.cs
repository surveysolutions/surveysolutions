using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.HeadquarterUserCommandValidatorTests
{
    internal class when_unarhiving_interviewer_of_archived_supervisor : HeadquarterUserCommandValidatorTestContext
    {
        Establish context = () =>
        {
            var supervisor = Create.UserDocument(isArchived: true);

            headquarterUserCommandValidatorser =
                CreateHeadquarterUserCommandValidatorWithUsers(supervisor);

            user = Create.User();
            user.ApplyEvent(Create.NewUserCreated(supervisorId: supervisor.PublicKey));
            user.ApplyEvent(Create.UserArchived());
        };

        Because of = () =>
            exception = Catch.Only<UserException>(() => headquarterUserCommandValidatorser.Validate(user, Create.UnarchiveUserCommand(user.EventSourceId)));

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_SupervisorArchived = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.SupervisorArchived);

        private static User user;
        private static UserException exception;
        private static HeadquarterUserCommandValidator headquarterUserCommandValidatorser;
    }
}