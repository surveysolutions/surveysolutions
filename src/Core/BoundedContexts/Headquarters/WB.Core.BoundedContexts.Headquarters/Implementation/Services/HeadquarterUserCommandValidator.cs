using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class HeadquarterUserCommandValidator : 
        ICommandValidator<User, CreateUserCommand>, 
        ICommandValidator<User, ArchiveUserCommad>, 
        ICommandValidator<User, UnarchiveUserCommand>
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviews;

        public HeadquarterUserCommandValidator(IQueryableReadSideRepositoryReader<UserDocument> users, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviews)
        {
            this.users = users;
            this.interviews = interviews;
        }

        public void Validate(User aggregate,CreateUserCommand command)
        {
            if (IsActiveUserExists(command.UserName))
                throw new UserException(String.Format("user name '{0}' is taken", command.UserName), UserDomainExceptionType.UserNameTakenByActiveUsers);

            if (IsArchivedUserExists(command.UserName))
                throw new UserException(String.Format("user name '{0}' is taken by archived users", command.UserName), UserDomainExceptionType.UserNameTakenByArchivedUsers);


            if (command.Roles.Contains(UserRoles.Operator))
            {
                ThrowIfInterviewerSupervisorIsArchived(command.Supervisor.Id);
            }
        }

        public void Validate(User aggregate, ArchiveUserCommad command)
        {
            var state = aggregate.CreateSnapshot();
            if (state.UserRoles.Contains(UserRoles.Operator))
            {
                var countOfInterviewsUserResposibleFor =
                    CountOfInterviewsInterviewerResposibleFor(aggregate.EventSourceId);

                if (countOfInterviewsUserResposibleFor > 0)
                {
                    throw new UserException(String.Format(
                        "Interviewer {0} is resposible for {1} interview(s) and can't be deleted", state.LoginName,
                        countOfInterviewsUserResposibleFor), UserDomainExceptionType.UserHasAssigments);
                }
            }
            else if (!state.UserRoles.Contains(UserRoles.Supervisor))
            {
                throw new UserException(String.Format("user in roles {0} can't be deleted", string.Join(",", state.UserRoles)), UserDomainExceptionType.RoleDoesntSupportDelete);
            }
        }

        public void Validate(User aggregate, UnarchiveUserCommand command)
        {
            var state = aggregate.CreateSnapshot();
            if (state.UserRoles.Contains(UserRoles.Operator))
            {
                ThrowIfInterviewerSupervisorIsArchived(state.UserSupervisorId);
            }
        }

        private bool IsUserActive(Guid userId)
        {
            var user = users.GetById(userId);
            if (user == null)
                return false;
            return !user.IsArchived;
        }

        private bool IsActiveUserExists(string userName)
        {
            return users.Query(_ => _.Where(u => !u.IsArchived && u.UserName.ToLower() == userName.ToLower()).Count() > 0);
        }

        private bool IsArchivedUserExists(string userName)
        {
            return users.Query(_ => _.Where(u => u.IsArchived && u.UserName.ToLower() == userName.ToLower()).Count() > 0);
        }

        private void ThrowIfInterviewerSupervisorIsArchived(Guid supervisorId)
        {
            if (!IsUserActive(supervisorId))
                throw new UserException("You can't unarchive interviewer until supervisor is archived",
                    UserDomainExceptionType.SupervisorArchived);
        }

        private int CountOfInterviewsInterviewerResposibleFor(Guid interviewerId)
        {
            return
                interviews.Query(
                    _ => _.Where(i => !i.IsDeleted && i.ResponsibleRole == UserRoles.Operator && i.ResponsibleId == interviewerId && (i.Status == InterviewStatus.InterviewerAssigned || i.Status == InterviewStatus.RejectedBySupervisor)).Count());
        }
    }
}