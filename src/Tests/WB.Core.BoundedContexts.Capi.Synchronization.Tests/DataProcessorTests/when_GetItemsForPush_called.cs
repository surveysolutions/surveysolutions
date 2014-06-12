using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.DataProcessorTests
{
    internal class when_GetItemsForPush_called : DataProcessorTestContext
    {
        Establish context = () =>
        {
            var changeLogShortRecords = new List<ChangeLogShortRecord>
            {
                new ChangeLogShortRecord(Guid.NewGuid(), Guid.NewGuid()),
                new ChangeLogShortRecord(Guid.NewGuid(), Guid.NewGuid())
            };

            changeLogManipulator = new Mock<IChangeLogManipulator>();
            changeLogManipulator.Setup(x => x.GetClosedDraftChunksIds()).Returns(changeLogShortRecords);
            changeLogManipulator.Setup(x => x.GetDraftRecordContent(Moq.It.IsAny<Guid>())).Returns<Guid>(key => key.ToString());

            dataProcessor = CreateDataProcessor(changeLogManipulator.Object);
        };

        Because of = () => result = dataProcessor.GetItemsForPush();

        It should_result_has_2_items = () => result.Count.ShouldEqual(2);

        It should_draft_record_content_be_requested_twice = () => changeLogManipulator.Verify(x => x.GetDraftRecordContent(Moq.It.IsAny<Guid>()), Times.Exactly(2));

        private static DataProcessor dataProcessor;
        private static IList<ChangeLogRecordWithContent> result;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
    }
}
