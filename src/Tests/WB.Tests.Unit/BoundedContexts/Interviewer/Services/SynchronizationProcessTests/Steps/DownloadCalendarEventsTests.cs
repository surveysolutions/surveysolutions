using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.Steps
{
    [TestOf(typeof(DownloadCalendarEvents))]
    public class DownloadCalendarEventsTests
    {
        [Test]
        public async Task when_no_calendar_events_to_sync()
        {
            var calendarStorage = new Mock<ICalendarEventStorage>();
            calendarStorage
                .Setup(x => x.LoadAll())
                .Returns(ArraySegment<CalendarEvent>.Empty);
            
            var eventStorage = new Mock<IEnumeratorEventStorage>();
            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(x => x.GetCalendarEventsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<CalendarEventApiView>()));

            var calendarEventRemoval = new Mock<ICalendarEventRemoval>();
            
            var downloadCalendarEventsSyncStep = DownloadCalendarEvents(calendarStorage.Object, 
                eventStorage.Object,
                syncService.Object,
                calendarEventRemoval.Object);
            
            //act
            await downloadCalendarEventsSyncStep.ExecuteAsync();

            //assert
            calendarStorage.Verify(x=>x.LoadAll(),Times.Once);
            eventStorage.Verify(x=> x.Read(It.IsAny<Guid>(), It.IsAny<int>()),Times.Never);
            calendarEventRemoval.Verify(x=>x.Remove(It.IsAny<Guid>()),Times.Never);
        }

        [Test]
        public async Task when_sync_a_local_calendar_event_not_returned_from_server()
        {
            var calendarEventId1 = Id.g1;
            var calendarEventId2 = Id.g2;
            
            var calendarStorage = new Mock<ICalendarEventStorage>();
            calendarStorage
                .Setup(x => x.LoadAll())
                .Returns(new List<CalendarEvent>()
                {
                    new CalendarEvent(){Id = calendarEventId1},
                    new CalendarEvent(){Id = calendarEventId2},
                });
            
            var eventStorage = new Mock<IEnumeratorEventStorage>();
            
            
            var syncService = new Mock<ISynchronizationService>();
            syncService.Setup(x => x.GetCalendarEventsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<CalendarEventApiView>()
                {
                    new CalendarEventApiView()
                    {
                        CalendarEventId = calendarEventId1, 
                        Sequence = 1
                    }
                }));
            /*syncService.Setup(x => x.GetCalendarEventStreamAsync(calendarEventId1, 1,
                It.IsAny<IProgress<TransferProgress>>(), It.IsAny<CancellationToken>())).Returns(
                Task.FromResult());*/

            var calendarEventRemoval = new Mock<ICalendarEventRemoval>();
            var downloadCalendarEventsSyncStep = DownloadCalendarEvents(calendarStorage.Object, 
                eventStorage.Object,
                syncService.Object,
                calendarEventRemoval.Object);
            
            //act
            await downloadCalendarEventsSyncStep.ExecuteAsync();

            //assert
            calendarStorage.Verify(x=>x.LoadAll(),Times.Once);
            eventStorage.Verify(x=> x.Read(It.IsAny<Guid>(), It.IsAny<int>()),Times.Never);
            calendarEventRemoval.Verify(x=>x.Remove(It.IsAny<Guid>()),Times.Exactly(2));
        }
        
        private DownloadCalendarEvents DownloadCalendarEvents(
            ICalendarEventStorage calendarEventStorage = null,
            IEnumeratorEventStorage eventStorage = null,    
            ISynchronizationService synchronizationService = null,
            ICalendarEventRemoval calendarEventRemoval = null)
        {
            var uploadCalendarEventsSyncStep = new DownloadCalendarEvents(1, 
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                eventStorage ?? Mock.Of<IEnumeratorEventStorage>(),
                calendarEventStorage ?? Mock.Of<ICalendarEventStorage>(),
                calendarEventRemoval ?? Mock.Of<ICalendarEventRemoval>(),
                Mock.Of<ILiteEventBus>(),
                Mock.Of<ILogger>());

            uploadCalendarEventsSyncStep.Context = new EnumeratorSynchonizationContext();
            uploadCalendarEventsSyncStep.Context.Progress = new Progress<SyncProgressInfo>();
            uploadCalendarEventsSyncStep.Context.Statistics = new SynchronizationStatistics();
            
            return uploadCalendarEventsSyncStep;
        }
    }
}
