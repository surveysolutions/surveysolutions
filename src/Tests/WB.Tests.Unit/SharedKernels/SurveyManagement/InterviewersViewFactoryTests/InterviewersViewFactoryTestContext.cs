using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewersViewFactoryTests
{
    [Subject(typeof(InterviewersViewFactory))]
    internal class InterviewersViewFactoryTestContext
    {
        protected static IInterviewersViewFactory CreateInterviewersViewFactory(IPlainStorageAccessor<UserDocument> readSideRepositoryIndexAccessor)
        {
            return new InterviewersViewFactory(readSideRepositoryIndexAccessor);
        }


        protected static IPlainStorageAccessor<UserDocument> CreateQueryableReadSideRepositoryReaderWithUsers(params UserDocument[] users)
        {
            var userStorage = new TestPlainStorage<UserDocument>();

            foreach (var user in users)
            {
                userStorage.Store(user, user.PublicKey.FormatGuid());
            }

            return userStorage;
        }

        protected static UserDocument CreateSupervisor(Guid userId, string userName)
        {
            return new UserDocument()
            {
                UserId = userId.ToString(),
                UserName = userName,
                Email = "",
                PublicKey = userId,
                Supervisor = new UserLight(),
                Roles = new HashSet<UserRoles>() { UserRoles.Supervisor }
            };
        }

        protected static UserDocument CreateHeadquarter(Guid userId, string userName)
        {
            return new UserDocument()
            {
                UserId = userId.ToString(),
                UserName = userName,
                Email = "",
                PublicKey = userId,
                Supervisor = new UserLight(),
                Roles = new HashSet<UserRoles>() { UserRoles.Headquarter }
            };
        }

        protected static UserDocument CreateInterviewer(Guid userId, UserDocument supervisor, string userName, string deviceId, bool isArchived = false)
        {
            return new UserDocument()
            {
                UserId = userId.ToString(),
                UserName = userName,
                Email = "",
                PublicKey = userId,
                DeviceId = deviceId,
                IsArchived = isArchived,
                Supervisor = new UserLight(supervisor.PublicKey, supervisor.UserName),
                Roles = new HashSet<UserRoles>() { UserRoles.Interviewer }
            };
        }
    }
}