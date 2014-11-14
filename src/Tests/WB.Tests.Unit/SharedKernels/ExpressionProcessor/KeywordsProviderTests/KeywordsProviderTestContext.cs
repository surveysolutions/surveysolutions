using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Tests.Unit.SharedKernels.ExpressionProcessor.KeywordsProviderTests
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
