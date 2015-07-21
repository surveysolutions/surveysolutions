using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_archive_user : UserTestContext
    {
        Establish context = () =>
        {
            eventContext = Create.EventContext();
            user = CreateUser();
            user.ApplyEvent(Create.NewUserCreated(role:UserRoles.Supervisor));
        };

        Because of = () =>
            user.Archive();

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_UserArchived_event = () =>
            eventContext.ShouldContainEvent<UserArchived>();

        private static EventContext eventContext;
        private static User user;
    }
}