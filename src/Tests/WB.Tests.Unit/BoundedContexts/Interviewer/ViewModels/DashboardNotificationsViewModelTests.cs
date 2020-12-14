using System;
using Moq;
using MvvmCross.Tests;
using Ncqrs;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels
{
    [TestOf(typeof(DashboardNotificationsViewModel))]
    [TestFixture]
    internal class DashboardNotificationsViewModelTests : MvxIoCSupportingTest
    {
        public DashboardNotificationsViewModelTests()
        {
            base.Setup();
        }

        [Test]
        public void when_checking_time_that_less_than_threshold()
        {
            long lastSyncHQTime = 10000000;
            var enumeratorSettings = new Mock<IEnumeratorSettings>();
            enumeratorSettings.Setup(x => x.LastHqSyncTimestamp).Returns(lastSyncHQTime);
            
            var clock = new Mock<IClock>();
            clock.Setup(x => x.DateTimeOffsetNow()).Returns(DateTimeOffset.FromUnixTimeSeconds(10000000 - 79 * 60));
            
            var notificationModel = CreateDashboardNotificationsViewModel(enumeratorSettings.Object, clock.Object);
            
            //act
            notificationModel.CheckTabletTimeAndWarn();
            
            Assert.That(notificationModel.IsNotificationPanelVisible, Is.False);
        }
        
        [Test]
        public void when_checking_time_thah_more_than_threshold()
        {
            long lastSyncHQTime = 10000000;
            var enumeratorSettings = new Mock<IEnumeratorSettings>();
            enumeratorSettings.Setup(x => x.LastHqSyncTimestamp).Returns(lastSyncHQTime);
            
            var clock = new Mock<IClock>();
            clock.Setup(x => x.DateTimeOffsetNow()).Returns(DateTimeOffset.FromUnixTimeSeconds(10000000 - 80 * 60));
            
            var notificationModel = CreateDashboardNotificationsViewModel(enumeratorSettings.Object, clock.Object);
            //act
            notificationModel.CheckTabletTimeAndWarn();
            
            Assert.That(notificationModel.IsNotificationPanelVisible, Is.True);
        }

        private static DashboardNotificationsViewModel CreateDashboardNotificationsViewModel(
            IEnumeratorSettings enumeratorSettings = null,
            IClock clock = null,
            IViewModelNavigationService viewModelNavigationService = null)
        {
            return new DashboardNotificationsViewModel(
                viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                enumeratorSettings?? Mock.Of<IEnumeratorSettings>(),
                clock?? Mock.Of<IClock>());
        }
    }
}
