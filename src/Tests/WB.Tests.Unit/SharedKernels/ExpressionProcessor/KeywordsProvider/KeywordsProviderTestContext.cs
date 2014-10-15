using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Tests.Unit.SharedKernels.ExpressionProcessor.KeywordsProvider
{
    internal class KeywordsProviderTestContext
    {
        protected static IKeywordsProvider CreateKeywordsProvider()
        {
            return new Core.SharedKernels.ExpressionProcessor.Implementation.Services.KeywordsProvider();
        }
    }
}
