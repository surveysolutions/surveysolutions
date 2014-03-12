using System.Security.Claims;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;

namespace WB.Core.BoundedContexts.Headquarters.Tests.AuthenticationTests.ApplicationUserTests
{
     [Subject(typeof(ApplicationUser))]
    public class when_removing_IsAdministrator_flag
    {
        private static ApplicationUser user;

        Establish context = () =>
        {
            user = new ApplicationUser("11");
            user.IsAdministrator = true;
        };

        Because of = () => user.IsAdministrator = false;

        It should_remove_role_claim_in_identity = () =>
            user.Claims.ShouldNotContain(claim => claim.ClaimType == ClaimTypes.Role && claim.ClaimValue == ApplicationRoles.Administrator);

        It should_return_false = () => user.IsAdministrator.ShouldBeFalse();
    }
}