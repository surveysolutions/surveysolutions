using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.MembershipProviderTests
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
