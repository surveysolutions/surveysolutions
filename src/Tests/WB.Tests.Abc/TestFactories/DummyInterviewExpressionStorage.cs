using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;

namespace WB.Tests.Abc.TestFactories
{
    public class DummyInterviewExpressionStorage : IInterviewExpressionStorage
    {
        public void Initialize(IInterviewStateForExpressions state)
        {
        }

        public IInterviewLevel GetLevel(Identity rosterIdentity)
        {
            return Mock.Of<IInterviewLevel>();
        }
    }
}
