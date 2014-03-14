using Machine.Specifications;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Authentication;

namespace WB.Core.BoundedContexts.Headquarters.Tests.AuthenticationTests.CustomPasswordValidatorTests
{
    [Subject(typeof(CustomPasswordValidator))]
    public class when_using_short_password
    {
        static CustomPasswordValidator validator;
        static IdentityResult identityResult;

        Establish context = () =>
        {
            validator = new CustomPasswordValidator(10);
        };

        Because of = async () => identityResult = await validator.ValidateAsync("100");

        It should_provide_failed_identity_result = () => identityResult.Succeeded.ShouldBeFalse();

        It should_provide_error_with_message = () => identityResult.Errors.ShouldContain(error => error.Contains("Password should be of length"));
    }
}