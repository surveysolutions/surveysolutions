using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Resources;
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
        ICommandValidator<User, UnarchiveUserCommand>,
        ICommandValidator<User, UnarchiveUserAndUpdateCommand>
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public HeadquarterUserCommandValidator(IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.users = users;
        }

        public void Validate(User aggregate, CreateUserCommand command)
        {
            if (IsActiveUserExists(command.UserName))
                throw new UserException(
                    String.Format(HeadquarterUserCommandValidatorMessages.UserNameIsTakenFormat, command.UserName),
                    UserDomainExceptionType.UserNameUsedByActiveUser);

            if (IsArchivedUserExists(command.UserName))
                throw new UserException(
                    String.Format(HeadquarterUserCommandValidatorMessages.UserNameIsTakenByArchivedUsersFormat,
                        command.UserName), UserDomainExceptionType.UserNameUsedByArchivedUser);
            
            if (command.Supervisor!=null)
                ThrowIfUserInRoleInterviewerAndSupervisorIsArchived(command.Roles, command.Supervisor.Id);
        }

        public void Validate(User aggregate, UnarchiveUserCommand command)
        {
            var state = aggregate.CreateSnapshot();
            ThrowIfUserInRoleInterviewerAndSupervisorIsArchived(state.UserRoles, state.UserSupervisorId);
        }

        public void Validate(User aggregate, UnarchiveUserAndUpdateCommand command)
        {
            var state = aggregate.CreateSnapshot();
            ThrowIfUserInRoleInterviewerAndSupervisorIsArchived(state.UserRoles, state.UserSupervisorId);
        }

        private bool IsActiveUserExists(string userName)
        {
            return users.Query(_ => _.Where(u => !u.IsArchived && u.UserName.ToLower() == userName.ToLower()).Count() > 0);
        }

        private bool IsArchivedUserExists(string userName)
        {
            return users.Query(_ => _.Where(u => u.IsArchived && u.UserName.ToLower() == userName.ToLower()).Count() > 0);
        }

        void ThrowIfUserInRoleInterviewerAndSupervisorIsArchived(UserRoles[] userRoles, Guid supervisorId)
        {
            if (!userRoles.Contains(UserRoles.Operator))
                return;

            var user = users.GetById(supervisorId);
            if (user == null || user.IsArchived)
                throw new UserException(
                    HeadquarterUserCommandValidatorMessages.YouCantUnarchiveInterviewerUntilSupervisorIsArchived,
                    UserDomainExceptionType.SupervisorArchived);
        }
    }
}