using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.CapiDataSynchronizationServiceTests
{
    internal class when_GetItemsForPush_called : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            var changeLogShortRecords = new List<ChangeLogShortRecord>
            {
                new ChangeLogShortRecord(Guid.NewGuid(), Guid.NewGuid()),
                new ChangeLogShortRecord(Guid.NewGuid(), Guid.NewGuid())
            };

            changeLogManipulator = new Mock<IChangeLogManipulator>();
            changeLogManipulator.Setup(x => x.GetClosedDraftChunksIds(userId)).Returns(changeLogShortRecords);
            changeLogManipulator.Setup(x => x.GetDraftRecordContent(Moq.It.IsAny<Guid>())).Returns<Guid>(key => key.ToString());

            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object);
        };

        Because of = () => result = capiDataSynchronizationService.GetItemsToPush(userId);

        It should_result_has_2_items = () => result.Count.ShouldEqual(2);

        It should_draft_record_content_be_requested_twice = () => changeLogManipulator.Verify(x => x.GetDraftRecordContent(Moq.It.IsAny<Guid>()), Times.Exactly(2));

        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static IList<ChangeLogRecordWithContent> result;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
    }
}
