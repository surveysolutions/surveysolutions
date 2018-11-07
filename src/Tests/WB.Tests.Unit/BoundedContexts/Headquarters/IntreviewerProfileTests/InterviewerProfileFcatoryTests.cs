using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.InterviewerProfiles;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Tests.Abc;
using WB.Tests.Abc.TestFactories;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.IntreviewerProfileTests
{
    public class InterviewerProfileFactoryTests
    {
        [Test]
        public void When_getting_profile_report_for_2_users()
        {
            var interviewersIdsToExport = new[] { Id.g1, Id.g2 };

            var users = new[] { Create.Entity.HqUser(Id.g1, Id.g3, userName: "u1"), Create.Entity.HqUser(Id.g2, Id.g3, userName: "u2"), Create.Entity.HqUser(Id.g3, userName: "super", role: UserRoles.Supervisor) };

            var userManager = Mock.Of<TestHqUserManager>(x => x.Users == users.AsQueryable());

            var deviceSyncInfos = new[] { Create.Entity.DeviceSyncInfo(Id.g1, "device1") };
            var deviceSyncInfoRepository = Mock.Of<IDeviceSyncInfoRepository>(x => x.GetLastSyncByInterviewersList(interviewersIdsToExport) == deviceSyncInfos);

            var factory = Create.Service.InterviewerProfileFactory(userManager: userManager, deviceSyncInfoRepository: deviceSyncInfoRepository);
            
            var report = factory.GetInterviewersReport(interviewersIdsToExport);

            Assert.That(report.Data.Length, Is.EqualTo(2));

            Assert.That(report.Data[0], Is.EquivalentTo(new object[]{ "u1", Id.g1, "super", "AppVersion", false, null, 0, 0, null, null, "device1",
                "DeviceSerialNumber", "DeviceType", "DeviceManufacturer", "DeviceModel", "DeviceBuildNumber", "DeviceLanguage", "Android AndroidSdkVersionName(25)",
                deviceSyncInfos[0].LastAppUpdatedDate, 14.15, 16.17, "AppOrientation", 88, "BatteryPowerSource", false,
                5242880, 2097152000, 52428800, 1073741824, 76546048,
                deviceSyncInfos[0].SyncDate, deviceSyncInfos[0].DeviceDate, "NetworkType", "NetworkSubType", 0, 0, 0, 10, 0, 0 }));

            Assert.That(report.Data[1], Is.EquivalentTo(new object[]{ "u2", Id.g2, "super", null, false, null, 0, 0, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, 0, 0, 0, 0, 0, 0 }));
        }

        [Test]
        public void When()
        {
            var inter = Create.Entity.HqUser(Id.g1, Id.g3, userName: "u1");
            var super = Create.Entity.HqUser(Id.g3, userName: "super", role: UserRoles.Supervisor);

            var userManager = Mock.Of<TestHqUserManager>(x 
                => x.FindByIdAsync(Id.g1) == Task.FromResult(inter)
                && x.FindByIdAsync(Id.g3) == Task.FromResult(super)
            );

            var deviceSyncInfo = Create.Entity.DeviceSyncInfo(Id.g1, "device1");
            var deviceSyncInfoRepository = Mock.Of<IDeviceSyncInfoRepository>(x => x.GetLastSuccessByInterviewerId(Id.g1) == deviceSyncInfo);

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
        }
    }
}
