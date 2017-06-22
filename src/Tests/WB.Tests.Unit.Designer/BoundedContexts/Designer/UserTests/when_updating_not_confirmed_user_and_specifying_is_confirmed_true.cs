using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UserTests
{
    internal class when_updating_not_confirmed_user_and_specifying_is_confirmed_true : UserTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            user = CreateUser();
            BecauseOf();
        }

        private void BecauseOf() =>
            user.Update(userName: "user name", comment: "some comment", email: "user@e.mail", passwordQuestion: "secret question", isLockedOut: true,
                isConfirmed: true, canImportOnHq: true);

        [NUnit.Framework.Test] public void should_set_IsConfirmed_to_true () =>
                user.IsConfirmed.ShouldEqual(true);

        private static User user;
    }
}