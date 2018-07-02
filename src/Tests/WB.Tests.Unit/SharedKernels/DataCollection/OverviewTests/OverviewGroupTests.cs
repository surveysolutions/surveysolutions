using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.OverviewTests
{
    [TestOf(typeof(OverviewGroup))]
    public class OverviewGroupTests
    {
        [Test]
        public void should_set_node_state_to_answered()
        {
            var group = new OverviewGroup(Create.Entity.InterviewTreeSection());

            Assert.That(group.State, Is.EqualTo(OverviewNodeState.Answered));
            Assert.That(group.IsAnswered, Is.True);
        }
    }
}
