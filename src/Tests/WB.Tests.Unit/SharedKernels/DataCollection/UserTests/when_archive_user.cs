using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_archive_user : UserTestContext
    {
        Establish context = () =>
        {
            user = Create.Entity.User();
            user.SetId(userId);
            user.Roles=new UserRoles[] { UserRoles.Supervisor};
        };

        Because of = () =>
            user.Archive();
        
        It should_user_be_archived = () =>
           user.IsArchived.ShouldBeTrue();

        private static User user;
        private static Guid userId = Guid.NewGuid();
    }
}