﻿using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    [TestOf(typeof(UserViewFactory))]
    internal class UserViewFactoryTestContext
    {
        protected static UserViewFactory CreateInterviewersViewFactory(IUserRepository userRepository)
            => new UserViewFactory(userRepository, Create.Storage.NewMemoryCache(), 
                Create.Storage.InMemoryPlainStorage<DeviceSyncInfo>(),
                Create.Service.WorkspaceContextAccessor());

        protected static IUserRepository CreateQueryableReadSideRepositoryReaderWithUsers( params HqUser[] users)
        {
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(x => x.Users).Returns(users.AsQueryable());
            return userRepository.Object;
        }
    }

    public static class UserRepositoryExtension
    {
        public static IUserRepository WithDeviceInfo(this IUserRepository users, params DeviceSyncInfo[] deviceSyncInfos)
        {
            var queryableDeviceSyncInfos = deviceSyncInfos.AsQueryable();
            var deviceSyncInfosMock = new Mock<DbSet<DeviceSyncInfo>>();
            deviceSyncInfosMock.As<IQueryable<DeviceSyncInfo>>().Setup(m => m.Provider).Returns(queryableDeviceSyncInfos.Provider);
            deviceSyncInfosMock.As<IQueryable<DeviceSyncInfo>>().Setup(m => m.Expression).Returns(queryableDeviceSyncInfos.Expression);
            deviceSyncInfosMock.As<IQueryable<DeviceSyncInfo>>().Setup(m => m.ElementType).Returns(queryableDeviceSyncInfos.ElementType);
            deviceSyncInfosMock.As<IQueryable<DeviceSyncInfo>>().Setup(m => m.GetEnumerator()).Returns(queryableDeviceSyncInfos.GetEnumerator());
            return users;
        }
    }
}
