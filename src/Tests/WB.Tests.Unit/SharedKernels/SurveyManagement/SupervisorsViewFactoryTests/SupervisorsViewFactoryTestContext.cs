using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Abc.Storage;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SupervisorsViewFactoryTests
{
    [Subject(typeof(SupervisorsViewFactory))]
    internal class SupervisorsViewFactoryTestContext
    {
        protected static ISupervisorsViewFactory CreateSupervisorsViewFactory(IPlainStorageAccessor<UserDocument> readSideRepositoryIndexAccessor)
        {
            return new SupervisorsViewFactory(readSideRepositoryIndexAccessor);
        }


        protected static IPlainStorageAccessor<UserDocument> CreateQueryableReadSideRepositoryReaderWithUsers(params UserDocument[] users)
        {
            var userStorage = new TestPlainStorage<UserDocument>();

            foreach (var user in users)
            {
                userStorage.Store(user, user.PublicKey);
            }

            return userStorage;
        }

        protected static UserDocument CreateSupervisor(Guid userId, string userName)
        {
            return new UserDocument()
            {
                UserId = userId.ToString(),
                UserName = userName,
                PublicKey = userId,
                Supervisor = new UserLight(),
                Roles = new HashSet<UserRoles>() { UserRoles.Supervisor } 
            };
        }

        protected static UserDocument CreateInterviewer(Guid userId, UserDocument supervisor, string userName, string deviceId, bool isArchived = false)
        {
            return new UserDocument()
            {
                UserId = userId.ToString(),
                UserName = userName,
                PublicKey = userId,
                DeviceId = deviceId,
                IsArchived = isArchived,
                Supervisor = new UserLight(supervisor.PublicKey, supervisor.UserName),
                Roles = new HashSet<UserRoles>() { UserRoles.Operator } 
            };
        }
    }
}