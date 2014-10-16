using WB.Core.SharedKernels.ExpressionProcessor.Implementation.Services;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

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
