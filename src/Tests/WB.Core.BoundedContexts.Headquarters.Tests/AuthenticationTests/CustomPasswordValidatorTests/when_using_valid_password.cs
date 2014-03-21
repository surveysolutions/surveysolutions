using Machine.Specifications;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Authentication;

namespace WB.Core.BoundedContexts.Headquarters.Tests.AuthenticationTests.CustomPasswordValidatorTests
{
    [Subject(typeof(CustomPasswordValidator))]
    public class when_using_valid_password
    {
        static CustomPasswordValidator validator;
        static IdentityResult identityResult;

        Establish context = () =>
        {
            validator = Create.CustomPasswordValidator();
        };

        Because of = async () => identityResult = await validator.ValidateAsync("Qwert12341");

        It should_provide_failed_identity_result = () => identityResult.Succeeded.ShouldBeTrue();
    }
}