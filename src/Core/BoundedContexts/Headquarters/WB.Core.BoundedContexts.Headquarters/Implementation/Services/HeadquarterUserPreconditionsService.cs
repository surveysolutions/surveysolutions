using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class SampleVerifier : ICommandValidator<User>
    {
        public SampleVerifier(IQueryableReadSideRepositoryReader<UserDocument> users)
        {
        }

        public void Validate(User aggregate)
        {
            throw new InvalidOperationException("User creation is no allowed");
        }
    }

    internal class HeadquarterUserPreconditionsService : IUserPreconditionsService
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviews;

        public HeadquarterUserPreconditionsService(IQueryableReadSideRepositoryReader<UserDocument> users, IQueryableReadSideRepositoryReader<InterviewSummary> interviews)
        {
            this.users = users;
            this.interviews = interviews;
        }

        public bool IsUserNameTakenByActiveUsers(string userName)
        {
            return users.Query(_ => _.Where(u =>!u.IsArchived && u.UserName.ToLower() == userName.ToLower()).Count() >0);
        }

        public bool IsUserNameTakenByArchivedUsers(string userName)
        {
            return users.Query(_ => _.Where(u => u.IsArchived && u.UserName.ToLower() == userName.ToLower()).Count() >0);
        }

        public int CountOfInterviewsInterviewerResposibleFor(Guid interviewerId)
        {
            return
                interviews.Query(
                    _ => _.Where(i => !i.IsDeleted && i.ResponsibleRole == UserRoles.Operator && i.ResponsibleId == interviewerId && (i.Status == InterviewStatus.InterviewerAssigned || i.Status == InterviewStatus.RejectedBySupervisor)).Count());
        }

        public bool IsUserActive(Guid userId)
        {
            var user = users.GetById(userId);
            if (user == null)
                return false;
            return !user.IsArchived;
        }
    }
}