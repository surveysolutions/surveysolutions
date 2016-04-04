using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_unarchive_active_user : UserTestContext
    {
        Establish context = () =>
        {
            var userId = Guid.NewGuid();
            Setup.InstanceToMockedServiceLocator(userDocumentStorage);
            var userDocument = Create.UserDocument(userId: userId, isArchived: false);
            userDocument.Roles.Add(UserRoles.Supervisor);
            userDocumentStorage.Store(userDocument, userId.FormatGuid());
            user = Create.User(userId);
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
        private static IPlainStorageAccessor<UserDocument> userDocumentStorage = new TestPlainStorage<UserDocument>();
    }
}