using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class HeadquarterUserCommandValidator : 
        ICommandValidator<User, UnarchiveUserCommand>
    {
        private readonly IPlainStorageAccessor<UserDocument> users;

        public HeadquarterUserCommandValidator(IPlainStorageAccessor<UserDocument> users)
        {
            this.users = users;
        }

        public void Validate(User aggregate, UnarchiveUserCommand command)
        {
            var user = users.GetById(aggregate.Id.FormatGuid());
            if (user.Supervisor != null)
                ThrowIfUserInRoleInterviewerAndSupervisorIsArchived(user.Roles.ToArray(), user.Supervisor.Id);
        }

        void ThrowIfUserInRoleInterviewerAndSupervisorIsArchived(UserRoles[] userRoles, Guid supervisorId)
        {
            if (!userRoles.Contains(UserRoles.Interviewer))
                return;

            var user = users.GetById(supervisorId.FormatGuid());
            if (user == null || user.IsArchived)
                throw new UserException(
                    HeadquarterUserCommandValidatorMessages.YouCantUnarchiveInterviewerUntilSupervisorIsArchived,
                    UserDomainExceptionType.SupervisorArchived);
        }
    }
}