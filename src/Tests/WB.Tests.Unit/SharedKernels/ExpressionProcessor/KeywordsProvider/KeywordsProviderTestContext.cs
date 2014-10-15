using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Tests.Unit.SharedKernels.ExpressionProcessor.KeywordsProvider
{
    internal class KeywordsProviderTestContext
    {
        protected static IKeywordsProvider CreateKeywordsProvider(ISubstitutionService substitutionService = null)
        {
            return new Core.SharedKernels.ExpressionProcessor.Implementation.Services.KeywordsProvider(substitutionService ?? CreateSubstitutionService());
        }

        protected static ISubstitutionService CreateSubstitutionService()
        {
            return new Core.SharedKernels.ExpressionProcessor.Implementation.Services.SubstitutionService();
        }

        

    }
}
