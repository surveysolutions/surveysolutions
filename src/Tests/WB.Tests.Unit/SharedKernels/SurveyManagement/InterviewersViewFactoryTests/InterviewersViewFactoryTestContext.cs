using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Views.Supervisor;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewersViewFactoryTests
{
    [Subject(typeof(InterviewersViewFactory))]
    internal class InterviewersViewFactoryTestContext
    {
        protected static IInterviewersViewFactory CreateInterviewersViewFactory(IQueryableReadSideRepositoryReader<UserDocument> readSideRepositoryIndexAccessor)
        {
            return new InterviewersViewFactory(readSideRepositoryIndexAccessor);
        }


        protected static IQueryableReadSideRepositoryReader<UserDocument> CreateQueryableReadSideRepositoryReaderWithUsers(params UserDocument[] users)
        {
            var userStorage = new TestInMemoryWriter<UserDocument>();

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
                Roles = new HashSet<UserRoles>() { UserRoles.Operator }
            };
        }
    }
}