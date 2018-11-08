using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Device;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Repositories
{
    [TestOf(typeof(DeviceSyncInfoRepository))]
    //[Ignore("If DbSet<DeviceSyncInfo> is marked as virtual, migration frameworks skips it. Either test, either migration should be fixed")]
    internal class DeviceSyncInfoRepositoryTests
    {
        private DeviceSyncInfoRepository Create(params DeviceSyncInfo[] data)
        {
            var queriableData = new List<DeviceSyncInfo>(data).AsQueryable();
            var dbSet = new Mock<DbSet<DeviceSyncInfo>>();

            dbSet.As<IQueryable<DeviceSyncInfo>>().Setup(m => m.Provider).Returns(queriableData.Provider);
            dbSet.As<IQueryable<DeviceSyncInfo>>().Setup(m => m.Expression).Returns(queriableData.Expression);
            dbSet.As<IQueryable<DeviceSyncInfo>>().Setup(m => m.ElementType).Returns(queriableData.ElementType);
            dbSet.As<IQueryable<DeviceSyncInfo>>().Setup(m => m.GetEnumerator()).Returns(() => queriableData.GetEnumerator());

            var dbContext = new Mock<HQPlainStorageDbContext>();
            dbContext.SetupGet(x => x.DeviceSyncInfo).Returns(dbSet.Object);
            return new DeviceSyncInfoRepository(dbContext.Object);
        } 

        [Test]
        public void when_getting_synchronization_activity()
        {
            //arrange
            var interviewerId = Guid.Parse("11111111111111111111111111111111");
            var deviceId = "111111";
            var twoDaysAgo = DateTime.UtcNow.AddDays(-2);
            var lastSyncDate = new DateTime(twoDaysAgo.Year, twoDaysAgo.Month, twoDaysAgo.Day, 13, 0, 0);

            var repository = this.Create(new[]
            {
                new DeviceSyncInfo
                {
                    InterviewerId = interviewerId,
                    DeviceId = deviceId,
                    SyncDate = lastSyncDate,
                    Statistics = new SyncStatistics
                    {
                        AssignmentsOnDeviceCount = 5,
                        NewAssignmentsCount = 1,
                        UploadedInterviewsCount = 1
                    }
                },
                new DeviceSyncInfo
                {
                    InterviewerId = interviewerId,
                    DeviceId = deviceId,
                    SyncDate = lastSyncDate.AddDays(1).AddHours(7),
                    Statistics = new SyncStatistics
                    {
                        AssignmentsOnDeviceCount = 10,
                        NewAssignmentsCount = 5,
                        UploadedInterviewsCount = 2
                    }
                },
                new DeviceSyncInfo
                {
                    InterviewerId = interviewerId,
                    DeviceId = deviceId,
                    SyncDate = lastSyncDate.AddDays(-8),
                    Statistics = new SyncStatistics
                    {
                        AssignmentsOnDeviceCount = 5,
                        NewAssignmentsCount = 1,
                        UploadedInterviewsCount = 1
                    }
                }
            });

            //act
            var result = repository.GetSynchronizationActivity(interviewerId);

            //assert
            Assert.That(result.Days.Length, Is.EqualTo(7));

            Assert.That(result.Days[5].Quarters[3].AllAssignmentsOnDeviceCount, Is.EqualTo(10));
            Assert.That(result.Days[5].Quarters[3].DownloadedAssigmentsCount, Is.EqualTo(5));
            Assert.That(result.Days[5].Quarters[3].UploadedInterviewsCount, Is.EqualTo(2));

            Assert.That(result.Days[4].Quarters[2].AllAssignmentsOnDeviceCount, Is.EqualTo(5));
            Assert.That(result.Days[4].Quarters[2].DownloadedAssigmentsCount, Is.EqualTo(1));
            Assert.That(result.Days[4].Quarters[2].UploadedInterviewsCount, Is.EqualTo(1));
        }
        [Test]
        public void when_getting_synchronization_activity_with_19_downloaded_assignments_and_1_uploaded_interview_then_downloaded_items_should_be_6_and_uploaded_1()
        {
            //arrange
            var interviewerId = Guid.Parse("11111111111111111111111111111111");
            var deviceId = "111111";
            var lastSyncDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 1, 0, 0);

            var repository = this.Create(new[]
            {
                new DeviceSyncInfo
                {
                    InterviewerId = interviewerId,
                    DeviceId = deviceId,
                    SyncDate = lastSyncDate,
                    Statistics = new SyncStatistics
                    {
                        AssignmentsOnDeviceCount = 0,
                        NewAssignmentsCount = 19,
                        UploadedInterviewsCount = 0
                    }
                },
                new DeviceSyncInfo
                {
                    InterviewerId = interviewerId,
                    DeviceId = deviceId,
                    SyncDate = lastSyncDate.AddMinutes(10),
                    Statistics = new SyncStatistics
                    {
                        AssignmentsOnDeviceCount = 19,
                        NewAssignmentsCount = 0,
                        UploadedInterviewsCount = 1
                    }
                }
            });

            //act
            var result = repository.GetSynchronizationActivity(interviewerId);

            //assert

            Assert.That(result.Days[6].Quarters[0].DownloadedAssignmentsInProportionCount, Is.EqualTo(5));
            Assert.That(result.Days[6].Quarters[0].UploadedInterviewsInProportionCount, Is.EqualTo(1));
        }

        [Test]
        public void when_getting_synchronization_activity_and_1_interviewer_has_many_devices()
        {
            //arrange
            var interviewerId = Guid.Parse("11111111111111111111111111111111");
            var lastSyncDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 1, 0, 0);

            var repository = this.Create(new[]
            {
                new DeviceSyncInfo
                {
                    InterviewerId = interviewerId,
                    DeviceId = Guid.NewGuid().ToString(),
                    SyncDate = lastSyncDate,
                    Statistics = new SyncStatistics{UploadedInterviewsCount = 1}
                },
                new DeviceSyncInfo
                {
                    InterviewerId = interviewerId,
                    DeviceId = Guid.NewGuid().ToString(),
                    SyncDate = lastSyncDate.AddDays(-2),
                    Statistics = new SyncStatistics{UploadedInterviewsCount = 1}
                }, 
                new DeviceSyncInfo
                {
                    InterviewerId = interviewerId,
                    DeviceId = Guid.NewGuid().ToString(),
                    SyncDate = lastSyncDate.AddDays(-4),
                    Statistics = new SyncStatistics{UploadedInterviewsCount = 1}
                }
            });

            //act
            var result = repository.GetSynchronizationActivity(interviewerId);

            //assert
            Assert.AreEqual(3, result.Days.Sum(x => x.Quarters.Sum(y => y.UploadedInterviewsCount)));
        }
    }
}
