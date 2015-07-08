using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Tests.Unit.SharedKernels.DataCollection.UserTests;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.HeadquarterUserCommandValidatorTests
{
    internal class when_arhiving_interviewer_with_assigments : HeadquarterUserCommandValidatorTestContext
    {
        Establish context = () =>
        {
            user = Create.User();
            var summaries = new TestInMemoryWriter<InterviewSummary>();
            summaries.Store(Create.InterviewSummary(responsibleId: user.EventSourceId,status:InterviewStatus.InterviewerAssigned), "id");

            headquarterUserCommandValidatorser =
                CreateHeadquarterUserCommandValidator(
                    interviews: summaries);
            user.ApplyEvent(Create.NewUserCreated());
        };

        private Because of = () =>
            exception =
                Catch.Only<UserException>(
                    () => headquarterUserCommandValidatorser.Validate(user, Create.ArchiveUserCommad(user.EventSourceId)));

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_UserHasAssigments = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.UserHasAssigments);

        private static HeadquarterUserCommandValidator headquarterUserCommandValidatorser;
        private static UserException exception;
        private static User user;
    }
}