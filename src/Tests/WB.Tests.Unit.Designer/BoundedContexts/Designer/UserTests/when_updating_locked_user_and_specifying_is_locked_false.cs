using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UserTests
{
    internal class when_updating_locked_user_and_specifying_is_locked_false : UserTestsContext
    {
        Establish context = () =>
        {
            user = CreateUser();
            user.IsLockedOut = true;
        };

        Because of = () =>
            user.Update(userName: "username", comment: null, email: null, passwordQuestion: null, isLockedOut: false, isConfirmed: false);

        It should_set_IsLockedOut_to_false = () =>
            user.IsLockedOut.ShouldEqual(false);

        private static User user;
    }
}