using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Eventing;
using NSubstitute;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using CalendarEvent = WB.Core.SharedKernels.Enumerator.Views.CalendarEvent;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.Steps
{
    [TestOf(typeof(UploadCalendarEvents))]
    public class UploadCalendarEventsTests
    {
        [Test]
        public async Task when_no_calendar_events_to_sync()
        {
            var calendarStorage = new Mock<ICalendarEventStorage>();
            calendarStorage
                .Setup(x => x.GetNotSynchedCalendarEventsInOrder())
                .Returns(ArraySegment<CalendarEvent>.Empty);
            
            var eventStorage = new Mock<IEnumeratorEventStorage>();
            
            var uploadCalendarEventsSyncStep = UploadCalendarEvents(calendarStorage.Object, eventStorage.Object);
            
            //act
            await uploadCalendarEventsSyncStep.ExecuteAsync();

            //assert
            calendarStorage.Verify(x=>x.GetNotSynchedCalendarEventsInOrder(),Times.Once);
            eventStorage.Verify(x=> x.Read(It.IsAny<Guid>(), It.IsAny<int>()),Times.Never);
        }
        
        [Test]
        public async Task when_synchronize_calendar_events()
        {
            var calendarEventId = Id.g1;
            
            var calendarStorage = new Mock<ICalendarEventStorage>();
            calendarStorage
                .Setup(x => x.GetNotSynchedCalendarEventsInOrder())
                .Returns(new[]{new CalendarEvent(){Id = calendarEventId}});

            var eventStorage = new Mock<IEnumeratorEventStorage>();
            eventStorage.Setup(x => x.Read(calendarEventId, It.IsAny<int>()))
                .Returns(new List<CommittedEvent>());

            eventStorage.Setup(x => x.GetLastEventKnownToHq(calendarEventId)).Returns(0);
            
            var syncService = new Mock<ISynchronizationService>();
            
            var uploadCalendarEventsSyncStep = UploadCalendarEvents(calendarStorage.Object, 
                eventStorage.Object, syncService.Object);
            //act
            await uploadCalendarEventsSyncStep.ExecuteAsync();

            //assert
            calendarStorage.Verify(x=> x.GetNotSynchedCalendarEventsInOrder(),Times.Once);
            eventStorage.Verify(x=> x.Read(calendarEventId, It.IsAny<int>()));
            syncService.Verify(x=> x.UploadCalendarEventAsync(
                It.IsAny<Guid>(),
                It.IsAny<CalendarEventPackageApiView>(),
                It.IsAny<IProgress<TransferProgress>>(),
                It.IsAny<CancellationToken>()));
            calendarStorage.Verify(x=> x.SetCalendarEventSyncedStatus(calendarEventId, true),
                Times.Once);
            eventStorage.Verify(x=> x.MarkAllEventsAsReceivedByHq(calendarEventId));
        }

        private UploadCalendarEvents UploadCalendarEvents(
            ICalendarEventStorage calendarEventStorage = null,
            IEnumeratorEventStorage eventStorage = null,    
            ISynchronizationService synchronizationService = null)
        {
            var logger = new Mock<ILogger>(); 
            var serializer = new Mock<ISerializer>();
            var calendarEventRemoval = new Mock<ICalendarEventRemoval>();
            
            
            var uploadCalendarEventsSyncStep = new UploadCalendarEvents(1, 
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                logger.Object,
                eventStorage ?? Mock.Of<IEnumeratorEventStorage>(),
                serializer.Object,
                calendarEventStorage ?? Mock.Of<ICalendarEventStorage>(),
                calendarEventRemoval.Object);

            uploadCalendarEventsSyncStep.Context = new EnumeratorSynchonizationContext();
            uploadCalendarEventsSyncStep.Context.Progress = new Progress<SyncProgressInfo>();
            uploadCalendarEventsSyncStep.Context.Statistics = new SynchronizationStatistics();
            
            return uploadCalendarEventsSyncStep;
        }
    }
}
