using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.InterviewerProfiles;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewerProfileTests
{
    public class InterviewerProfileFactoryTests
    {
        [Test]
        public async Task When_getting_traffic_usage()
        {

            var trafficUsage = new List<InterviewerDailyTrafficUsage>
            {
                Create.Entity.InterviewerDailyTrafficUsage(1024*2, 1024*1, 2018, 9, 30),
                Create.Entity.InterviewerDailyTrafficUsage(1024*3, 1024*1, 2018, 10, 1),
                Create.Entity.InterviewerDailyTrafficUsage(1024*4, 1024*0, 2018, 10, 5),
                Create.Entity.InterviewerDailyTrafficUsage(1024*5, 1024*2, 2018, 10, 6),
                Create.Entity.InterviewerDailyTrafficUsage(1024*2, 1024*4, 2018, 11, 29),
                Create.Entity.InterviewerDailyTrafficUsage(1024*3, 1024*7, 2018, 11, 30),
                Create.Entity.InterviewerDailyTrafficUsage(1024*4, 1024*5, 2019, 1, 1),
                Create.Entity.InterviewerDailyTrafficUsage(1024*6, 1024*1, 2019, 1, 2),
                Create.Entity.InterviewerDailyTrafficUsage(1024*6, 1024*1, 2020, 1, 2)
            };

            var deviceSyncInfoRepository = Mock.Of<IDeviceSyncInfoRepository>(x => 
                x.GetTrafficUsageForInterviewer(Id.g1) == trafficUsage &&
                x.GetTotalTrafficUsageForInterviewer(Id.g1) == Task.FromResult(1024 * 48l));

            var factory = Create.Service.InterviewerProfileFactory(deviceSyncInfoRepository: deviceSyncInfoRepository);
            
            var monthlyUsage = await factory.GetInterviewerTrafficUsageAsync(Id.g1);

            Assert.That(monthlyUsage.TrafficUsages.Count(), Is.EqualTo(5));
            Assert.That(monthlyUsage.TrafficUsages.ElementAt(0).Month, Is.EqualTo("Sep"));
            Assert.That(monthlyUsage.TrafficUsages.ElementAt(1).Month, Is.EqualTo("Oct"));
            Assert.That(monthlyUsage.TrafficUsages.ElementAt(2).Month, Is.EqualTo("Nov 18"));
            Assert.That(monthlyUsage.TrafficUsages.ElementAt(3).Month, Is.EqualTo("Jan 19"));
            Assert.That(monthlyUsage.TrafficUsages.ElementAt(4).Month, Is.EqualTo("Jan 20"));

            foreach (var monthlyTrafficUsage in monthlyUsage.TrafficUsages)
            {
                Assert.That(monthlyTrafficUsage.DailyUsage.Count, Is.AtLeast(3));
            }

            CollectionAssert.AreEqual(monthlyUsage.TrafficUsages.ElementAt(0).DailyUsage.Select(x => x.Day), new [] {28, 29, 30});
            CollectionAssert.IsOrdered(monthlyUsage.TrafficUsages.ElementAt(0).DailyUsage.Select(x => x.Day));

            CollectionAssert.AreEqual(monthlyUsage.TrafficUsages.ElementAt(2).DailyUsage.Select(x => x.Day), new [] {28, 29, 30});
            CollectionAssert.IsOrdered(monthlyUsage.TrafficUsages.ElementAt(2).DailyUsage.Select(x => x.Day));

            CollectionAssert.AreEqual(monthlyUsage.TrafficUsages.ElementAt(3).DailyUsage.Select(x => x.Day), new [] {1, 2, 31});
            CollectionAssert.IsOrdered(monthlyUsage.TrafficUsages.ElementAt(3).DailyUsage.Select(x => x.Day));

            CollectionAssert.AreEqual(monthlyUsage.TrafficUsages.ElementAt(4).DailyUsage.Select(x => x.Day), new [] {2, 30, 31});
            CollectionAssert.IsOrdered(monthlyUsage.TrafficUsages.ElementAt(4).DailyUsage.Select(x => x.Day));

            Assert.That(monthlyUsage.TotalTrafficUsed, Is.EqualTo(48));
            Assert.That(monthlyUsage.MaxDailyUsage, Is.EqualTo(10));
            
        }
        [Test]
        public void When_getting_profile_report_for_2_users()
        {
            var interviewersIdsToExport = new[] { Id.g1, Id.g2 };

            var users = new[]
            {
                Create.Entity.HqUser(Id.g1, Id.g3, userName: "u1"), 
                Create.Entity.HqUser(Id.g2, Id.g3, userName: "u2"), 
                Create.Entity.HqUser(Id.g3, userName: "super", role: UserRoles.Supervisor)
            };

            var userManager = Mock.Of<TestHqUserManager>(x => x.Users == users.AsQueryable());

            var deviceSyncInfos = new[] { Create.Entity.DeviceSyncInfo(Id.g1, "device1") };
            var trafficUsage = new Dictionary<Guid, long>
            {
                { Id.g1, 30000 }
            };

            var deviceSyncInfoRepository = Mock.Of<IDeviceSyncInfoRepository>(x 
                => x.GetLastSyncByInterviewersList(interviewersIdsToExport) == deviceSyncInfos 
                && x.GetInterviewersTrafficUsage(interviewersIdsToExport) == trafficUsage);

            var factory = Create.Service.InterviewerProfileFactory(userManager: userManager, deviceSyncInfoRepository: deviceSyncInfoRepository);
            
            var report = factory.GetInterviewersReport(interviewersIdsToExport);

            Assert.That(report.Data.Length, Is.EqualTo(2));

            Assert.That(report.Data[0], Is.EquivalentTo(new object[]{ "u1", Id.g1, "super", "AppVersion", false, null, 0, 0, null, null, "device1",
                "DeviceSerialNumber", "DeviceType", "DeviceManufacturer", "DeviceModel", "DeviceBuildNumber", 30000, "DeviceLanguage", "Android AndroidSdkVersionName(25)",
                deviceSyncInfos[0].LastAppUpdatedDate, 14.15, 16.17, "AppOrientation", 88, "BatteryPowerSource", false,
                5242880, 2097152000, 52428800, 1073741824, 76546048,
                deviceSyncInfos[0].SyncDate, deviceSyncInfos[0].DeviceDate, "NetworkType", "NetworkSubType", 0, 0, 0, 10, 0, 0 }));

            Assert.That(report.Data[1], Is.EquivalentTo(new object[]{ "u2", Id.g2, "super", null, false, null, 0, 0, null, null,
                null, null, null, null, null, null, 0, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, 0, 0, 0, 0, 0, 0 }));
        }

        [Test]
        public void When_getting_interviewer_profile()
        {
            var inter = Create.Entity.HqUser(Id.g1, Id.g3, userName: "u1");
            var super = Create.Entity.HqUser(Id.g3, userName: "super", role: UserRoles.Supervisor);

            var userManager = Mock.Of<TestHqUserManager>(x 
                => x.FindByIdAsync(Id.g1) == Task.FromResult(inter)
                && x.FindByIdAsync(Id.g3) == Task.FromResult(super)
            );

            var deviceSyncInfo = Create.Entity.DeviceSyncInfo(Id.g1, "device1");
            var deviceSyncInfoRepository = Mock.Of<IDeviceSyncInfoRepository>(x 
                => x.GetLastSuccessByInterviewerId(Id.g1) == deviceSyncInfo
                && x.GetTotalTrafficUsageForInterviewer(Id.g1) == Task.FromResult(2*1024l));

            var factory = Create.Service.InterviewerProfileFactory(userManager: userManager, deviceSyncInfoRepository: deviceSyncInfoRepository);

            InterviewerProfileModel profile = factory.GetInterviewerProfileAsync(Id.g1).Result;

            Assert.That(profile, Is.Not.Null);

            Assert.That(profile.InterviewerId, Is.EqualTo(Id.g1));
            Assert.That(profile.DeviceId, Is.EqualTo("device1"));
            Assert.That(profile.LastSurveySolutionsUpdatedDate, Is.EqualTo(deviceSyncInfo.LastAppUpdatedDate));
            Assert.That(profile.DeviceModel, Is.EqualTo("DeviceModel"));
            Assert.That(profile.DeviceType, Is.EqualTo("DeviceType"));
            Assert.That(profile.AndroidVersion, Is.EqualTo("Android AndroidSdkVersionName(25)"));
            Assert.That(profile.DeviceLanguage, Is.EqualTo("DeviceLanguage"));
            Assert.That(profile.DeviceBuildNumber, Is.EqualTo("DeviceBuildNumber"));
            Assert.That(profile.DeviceSerialNumber, Is.EqualTo("DeviceSerialNumber"));
            Assert.That(profile.DeviceManufacturer, Is.EqualTo("DeviceManufacturer"));
            Assert.That(profile.DatabaseSizeInBytes, Is.EqualTo(73 * 1024 * 1024));
            Assert.That(profile.AndroidVersion, Is.EqualTo("Android AndroidSdkVersionName(25)"));
            Assert.That(profile.TabletTimeAtTeBeginningOfSync, Is.EqualTo(deviceSyncInfo.DeviceDate));
            Assert.That(profile.ServerTimeAtTheBeginningOfSync, Is.EqualTo(deviceSyncInfo.SyncDate));
            Assert.That(profile.SurveySolutionsVersion, Is.EqualTo("AppVersion"));
            Assert.That(profile.DeviceOrientation, Is.EqualTo("AppOrientation"));
            Assert.That(profile.ConnectionSubType, Is.EqualTo("NetworkSubType"));
            Assert.That(profile.ConnectionType, Is.EqualTo("NetworkType"));
            Assert.That(profile.BatteryStatus, Is.EqualTo(88));
            Assert.That(profile.BatteryPowerSource, Is.EqualTo("BatteryPowerSource"));
            Assert.That(profile.IsPowerSaveMode, Is.EqualTo(false));
            Assert.That(profile.DeviceLocationOrLastKnownLocationLat, Is.EqualTo(14.15));
            Assert.That(profile.DeviceLocationOrLastKnownLocationLon, Is.EqualTo(16.17));
            Assert.That(profile.AssignmentsThatHaveBeenStarted, Is.EqualTo(10));
            Assert.That(profile.RamFreeInBytes, Is.EqualTo(50 * 1024 * 1024));
            Assert.That(profile.RamTotalInBytes, Is.EqualTo(1024 * 1024 * 1024));
            Assert.That(profile.StorageFreeInBytes, Is.EqualTo(5 * 1024 * 1024));
            Assert.That(profile.StorageTotalInBytes, Is.EqualTo(2000 * 1024 * 1024));
            Assert.That(profile.TrafficUsed, Is.EqualTo(2));
        }
    }
}
