using System.Security.Claims;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;

namespace WB.Core.BoundedContexts.Headquarters.Tests.AuthenticationTests.ApplicationUserTests
{
    [Subject(typeof(ApplicationUser))]
    public class when_setting_IsAdministrator_flag
    {
        private static ApplicationUser user;

        Establish context = () => 
        {
            user = new ApplicationUser("11");
        };

        Because of = () => user.IsAdministrator = true;

        It should_add_role_claim_to_identity = () => 
            user.Claims.ShouldContain(claim => claim.ClaimType == ClaimTypes.Role && claim.ClaimValue == ApplicationRoles.Administrator);

        It should_return_true = () => user.IsAdministrator.ShouldBeTrue();
    }
}