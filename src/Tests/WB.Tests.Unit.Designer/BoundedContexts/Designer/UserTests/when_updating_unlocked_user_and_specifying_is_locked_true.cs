using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UserTests
{
    internal class when_updating_unlocked_user_and_specifying_is_locked_true : UserTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            user = CreateUser();
        }

        private void BecauseOf() =>
                user.Update(userName: "username", comment: null, email: null, passwordQuestion: null, isLockedOut: true, isConfirmed: false, canImportOnHq: false);

        [NUnit.Framework.Test] public void should_set_IsLockedOut_to_true () =>
                user.IsLockedOut.ShouldEqual(true);

        private static User user;
    }
}