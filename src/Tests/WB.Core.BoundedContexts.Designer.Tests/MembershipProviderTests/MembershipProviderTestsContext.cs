using Machine.Specifications;
using WB.UI.Shared.Web.MembershipProvider.Accounts;

namespace WB.Core.BoundedContexts.Designer.Tests.MembershipProviderTests
{
    [Subject(typeof(MembershipProvider))]
    internal class MembershipProviderTestsContext
    {
        protected static MembershipProvider CreateMembershipProvider()
        {
            return new MembershipProvider();
        }
    }
}
