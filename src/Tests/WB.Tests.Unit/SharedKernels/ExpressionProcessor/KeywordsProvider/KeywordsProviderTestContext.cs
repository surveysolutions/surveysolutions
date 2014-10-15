using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Tests.Unit.SharedKernels.ExpressionProcessor.KeywordsProvider
{
    internal class KeywordsProviderTestContext
    {
        protected static IKeywordsProvider CreateKeywordsProvider()
        {
            var substitutionService = new Core.SharedKernels.ExpressionProcessor.Implementation.Services.SubstitutionService();
            return new Core.SharedKernels.ExpressionProcessor.Implementation.Services.KeywordsProvider(substitutionService);
        }
    }
}
