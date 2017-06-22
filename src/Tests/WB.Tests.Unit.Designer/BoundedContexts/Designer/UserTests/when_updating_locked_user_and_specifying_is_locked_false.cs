using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UserTests
{
    internal class when_updating_locked_user_and_specifying_is_locked_false : UserTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            user = CreateUser();
            user.IsLockedOut = true;
        }

        private void BecauseOf() =>
            user.Update(userName: "username", comment: null, email: null, passwordQuestion: null, isLockedOut: false, isConfirmed: false, canImportOnHq: false);

        [NUnit.Framework.Test] public void should_set_IsLockedOut_to_false () =>
            user.IsLockedOut.ShouldEqual(false);

        private static User user;
    }
}