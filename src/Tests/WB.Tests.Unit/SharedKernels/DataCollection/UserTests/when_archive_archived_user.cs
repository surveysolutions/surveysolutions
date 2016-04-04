using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_archive_archived_user : UserTestContext
    {
        Establish context = () =>
        {
            Setup.InstanceToMockedServiceLocator(userDocumentStorage);
            var userDocument = Create.UserDocument(userId: userId, isArchived:true);
            userDocumentStorage.Store(userDocument, userId.FormatGuid());
            user = Create.User();
            user.SetId(userId);
        };

        Because of = () =>
            exception = Catch.Only<UserException>(() => user.Archive());

        It should_raise_UserException_event = () =>
            exception.ShouldNotBeNull();

        It should_raise_UserException_with_type_equal_UserArchived = () =>
            exception.ExceptionType.ShouldEqual(UserDomainExceptionType.UserArchived);

        private static User user;
        private static Guid userId = Guid.NewGuid();
        private static IPlainStorageAccessor<UserDocument> userDocumentStorage = new TestPlainStorage<UserDocument>();
        private static UserException exception;
    }
}