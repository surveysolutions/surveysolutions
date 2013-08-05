using WB.Core.SharedKernels.DataCollection.Implementation.Services;

namespace WB.Core.SharedKernels.DataCollection.Tests.ExpressionProcessorTests
{
    internal class ExpressionProcessorTestsContext
    {
        protected static ExpressionProcessor CreateExpressionProcessor()
        {
            return new ExpressionProcessor();
        }
    }
}