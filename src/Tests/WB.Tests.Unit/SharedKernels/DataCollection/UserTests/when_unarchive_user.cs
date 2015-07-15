using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection.UserTests
{
    internal class when_unarchive_user : UserTestContext
    {
        Establish context = () =>
        {
            eventContext = Create.EventContext();
            user = CreateUser();
            user.ApplyEvent(Create.NewUserCreated(role: UserRoles.Supervisor));
            user.ApplyEvent(Create.UserArchived());
        };

        Because of = () =>
            user.Unarchive();

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_UserUnarchived_event = () =>
            eventContext.ShouldContainEvent<UserUnarchived>();

        private static EventContext eventContext;
        private static User user;
    }
}