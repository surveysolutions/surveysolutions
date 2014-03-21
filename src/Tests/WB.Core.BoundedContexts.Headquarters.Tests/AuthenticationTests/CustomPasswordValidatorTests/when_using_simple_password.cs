using Machine.Specifications;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Authentication;

namespace WB.Core.BoundedContexts.Headquarters.Tests.AuthenticationTests.CustomPasswordValidatorTests
{
    [Subject(typeof(CustomPasswordValidator))]
    public class when_using_simple_password
    {
        static CustomPasswordValidator validator;
        static IdentityResult identityResult;

        Establish context = () =>
        {
            validator = Create.CustomPasswordValidator(0, @"^(?=.*[a-z])(?=.*[0-9])(?=.*[A-Z]).*$");
        };

        Because of = async () => identityResult = await validator.ValidateAsync("something");

        It should_provide_failed_identity_result = () => identityResult.Succeeded.ShouldBeFalse();

        It should_provide_error_with_message = () => identityResult.Errors.ShouldContain(error => error.Contains("Password must contain at least one number, one upper case character and one lower case character")); 
    }
}