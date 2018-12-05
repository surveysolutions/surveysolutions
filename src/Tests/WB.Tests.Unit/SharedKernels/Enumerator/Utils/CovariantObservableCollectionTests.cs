using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils
{
    public class CovariantObservableCollectionTests
    {
        [Test]
        public void should_be_able_to_suspend_events()
        {
            var list = new CovariantObservableCollection<int>();
            int eventRaisedCount = 0;

            list.CollectionChanged += (sender, args) => eventRaisedCount++;

            list.SuspendCollectionChanged();
            list.Add(1);
            list.Add(2);

            list.ResumeCollectionChanged();

            Assert.That(eventRaisedCount, Is.EqualTo(1));
        }
    }
}
