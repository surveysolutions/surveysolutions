using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserDenormalizerTests
{
    [Subject(typeof(UserDenormalizer))]
    class UserDenormalizerTests
    {
        protected static UserDenormalizer CreatevUserDenormalizer(IReadSideRepositoryWriter<UserDocument> users = null)
        {
            return new UserDenormalizer(users ?? Mock.Of<IReadSideRepositoryWriter<UserDocument>>());
        }

        protected static UserDocument CreateUser(Guid userId, List<DeviceInfo> history = null)
        {
            return new UserDocument
            {
                PublicKey = userId,
                UserName = "Vasya",
                Email = "vasya@the.best",
                Roles = new List<UserRoles>{ UserRoles.Operator },
                DeviceChangingHistory = history ?? new List<DeviceInfo>()
            };
        }

        protected static DeviceInfo CreateDeviceInfo(string deviceId, DateTime date)
        {
            return new DeviceInfo
            {
                DeviceId = deviceId,
                Date = date
            };
        }
    }
}
