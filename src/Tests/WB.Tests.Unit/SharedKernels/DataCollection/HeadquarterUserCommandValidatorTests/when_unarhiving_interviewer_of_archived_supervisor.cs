using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
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
            Setup.InstanceToMockedServiceLocator(userDocumentStorage);

            var supervisor = Create.Entity.UserDocument(isArchived: true);
            userDocumentStorage.Store(supervisor, supervisor.PublicKey.FormatGuid());
            headquarterUserCommandValidatorser =
                CreateHeadquarterUserCommandValidator(userDocumentStorage);

            user = Create.Entity.User();
            var userDocument = Create.Entity.UserDocument(userId: Guid.NewGuid(), isArchived: true, supervisorId: supervisor.PublicKey);
            userDocument.Roles.Add(UserRoles.Interviewer);
            userDocumentStorage.Store(userDocument, userDocument.PublicKey.FormatGuid());
            user.SetId(userDocument.PublicKey);
        };

        Because of = () =>
            exception = Catch.Only<UserException>(() => headquarterUserCommandValidatorser.Validate(user, Create.Command.UnarchiveUserCommand(user.Id)));

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_SupervisorArchived = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.SupervisorArchived);

        private static User user;
        private static UserException exception;
        private static HeadquarterUserCommandValidator headquarterUserCommandValidatorser;
        private static IPlainStorageAccessor<UserDocument> userDocumentStorage = new TestPlainStorage<UserDocument>();
    }
}