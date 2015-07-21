using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Unit.SharedKernels.DataCollection.UserTests;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.HeadquarterUserCommandValidatorTests
{
    internal class when_interviewer_is_creating_for_archived_supervisor : HeadquarterUserCommandValidatorTestContext
    {
        Establish context = () =>
        {
            supervisor = Create.UserDocument(isArchived: true);

            headquarterUserCommandValidatorser =
                CreateHeadquarterUserCommandValidatorWithUsers(supervisor);
        };

        Because of = () =>
            exception =
                Catch.Only<UserException>(
                    () => headquarterUserCommandValidatorser.Validate(null, Create.CreateUserCommand(userName:"inter", supervisorId: supervisor.PublicKey)));

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_SupervisorArchived = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.SupervisorArchived);

        private static UserException exception;
        private static UserDocument supervisor;
        private static HeadquarterUserCommandValidator headquarterUserCommandValidatorser;
    }
}