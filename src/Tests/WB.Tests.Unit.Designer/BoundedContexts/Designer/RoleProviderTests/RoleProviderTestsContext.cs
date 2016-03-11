using Machine.Specifications;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.RoleProviderTests
{
    [Subject(typeof(RoleProvider))]
    internal class RoleProviderTestsContext
    {
        protected static RoleProvider CreateRoleProvider()
        {

            return new RoleProvider();
        }
    }
}
