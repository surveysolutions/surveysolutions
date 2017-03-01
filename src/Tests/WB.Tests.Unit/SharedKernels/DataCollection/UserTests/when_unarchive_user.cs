using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_unarchive_user : UserTestContext
    {
        Establish context = () =>
        {
            user = Create.Entity.User();
            user.SetId(userId);
            user.Roles = new UserRoles[] { UserRoles.Supervisor };
            user.IsArchived = true;
        };

        Because of = () =>
            user.Unarchive();

        It should_unarchive_user_document = () =>
            user.IsArchived.ShouldBeFalse();
        
        private static User user;
        private static Guid userId = Guid.NewGuid();
    }
}