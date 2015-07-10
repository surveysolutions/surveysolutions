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
                throw new UserException(String.Format("user name '{0}' is taken", command.UserName), UserDomainExceptionType.UserNameUsedByActiveUser);

            if (IsArchivedUserExists(command.UserName))
                throw new UserException(String.Format("user name '{0}' is taken by archived users", command.UserName), UserDomainExceptionType.UserNameUsedByArchivedUser);


            if (command.Roles.Contains(UserRoles.Operator))
            {
                ThrowIfInterviewerSupervisorIsArchived(command.Supervisor.Id);
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
            var user = users.GetById(supervisorId);
            if (user == null || user.IsArchived)
                throw new UserException("You can't unarchive interviewer until supervisor is archived",
                    UserDomainExceptionType.SupervisorArchived);
        }
    }
}