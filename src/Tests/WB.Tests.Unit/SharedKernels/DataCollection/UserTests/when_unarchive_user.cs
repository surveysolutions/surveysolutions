using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_unarchive_user : UserTestContext
    {
        Establish context = () =>
        {
            Setup.InstanceToMockedServiceLocator(userDocumentStorage);
            var userDocument = Create.UserDocument(userId: userId, isArchived: true);
            userDocument.Roles.Add(UserRoles.Supervisor);
            userDocumentStorage.Store(userDocument, userId.FormatGuid());
            user = Create.User();
            user.SetId(userId);
        };

        Because of = () =>
            user.Unarchive();

        It should_unarchive_user_document = () =>
            userDocumentStorage.GetById(userId.FormatGuid()).IsArchived.ShouldBeFalse();
        
        private static User user;
        private static Guid userId = Guid.NewGuid();
        private static IPlainStorageAccessor<UserDocument> userDocumentStorage=new TestPlainStorage<UserDocument>();
    }
}