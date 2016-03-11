using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;


namespace WB.Tests.Unit.BoundedContexts.Designer.KeywordsProviderTests
{
    internal class KeywordsProviderTestContext
    {
        protected static IKeywordsProvider CreateKeywordsProvider(ISubstitutionService substitutionService = null)
        {
            return new KeywordsProvider(substitutionService ?? CreateSubstitutionService());
        }

        protected static ISubstitutionService CreateSubstitutionService()
        {
            return new SubstitutionService();
        }
    }
}
