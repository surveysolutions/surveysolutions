using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_arhiving_observer : UserTestContext
    {
        Establish context = () =>
        {
            Setup.InstanceToMockedServiceLocator(userDocumentStorage);
            var userDocument = Create.UserDocument(userId: userId);
            userDocument.Roles.Add(UserRoles.Observer);
            userDocumentStorage.Store(userDocument, userId.FormatGuid());
            user = Create.User();
            user.SetId(userId);
        };

        Because of = () =>
            exception = Catch.Only<UserException>(() => user.Archive());

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_RoleDoesntSupportDelete = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.RoleDoesntSupportDelete);

        private static User user;
        private static Guid userId = Guid.NewGuid();
        private static IPlainStorageAccessor<UserDocument> userDocumentStorage = new TestPlainStorage<UserDocument>();
        private static UserException exception;
    }
}