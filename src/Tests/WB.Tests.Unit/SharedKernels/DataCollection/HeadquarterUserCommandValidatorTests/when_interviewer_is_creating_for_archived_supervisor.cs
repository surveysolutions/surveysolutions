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
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.HeadquarterUserCommandValidatorTests
{
    internal class when_interviewer_is_creating_for_archived_supervisor : HeadquarterUserCommandValidatorTestContext
    {
        private Establish context = () =>
        {
            var supervisor = Create.UserDocument(isArchived: true);
            headquarterUserCommandValidatorser =
                CreateHeadquarterUserCommandValidator(
                    users:
                        Mock.Of<IQueryableReadSideRepositoryReader<UserDocument>>(
                            _ => _.GetById(Moq.It.IsAny<string>()) == supervisor));
        };

        private Because of = () =>
            exception =
                Catch.Only<UserException>(
                    () => headquarterUserCommandValidatorser.Validate(null, Create.CreateUserCommand()));

        private It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        private It should_raise_UserException_with_type_equal_SupervisorArchived = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.SupervisorArchived);

        private static UserException exception;
        private static HeadquarterUserCommandValidator headquarterUserCommandValidatorser;
    }
}