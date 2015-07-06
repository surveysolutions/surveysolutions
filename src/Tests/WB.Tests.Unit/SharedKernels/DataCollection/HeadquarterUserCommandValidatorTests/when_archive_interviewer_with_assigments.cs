using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Tests.Unit.SharedKernels.DataCollection.UserTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.HeadquarterUserCommandValidatorTests
{
    internal class when_archive_interviewer_with_assigments : HeadquarterUserCommandValidatorTestContext
    {
        Establish context = () =>
        {
            headquarterUserCommandValidatorser =
                CreateHeadquarterUserCommandValidator(
                    interviews:
                        Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(
                            _ => _.Query<int>(Moq.It.IsAny<Func<IQueryable<InterviewSummary>, int>>()) == 1));
            user = CreateUser();
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