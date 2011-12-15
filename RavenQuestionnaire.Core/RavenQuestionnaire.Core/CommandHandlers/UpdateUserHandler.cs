using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class UpdateUserHandler : ICommandHandler<UpdateUserCommand>
    {
        private IUserRepository _repository;
        private ILocationRepository _locationalRepository;
        public UpdateUserHandler(IUserRepository repository, ILocationRepository locationRepository)
        {
            _repository = repository;
            _locationalRepository = locationRepository;
        }

        #region Implementation of ICommandHandler<UpdateUserCommand>

        public void Handle(UpdateUserCommand command)
        {
            User user = _repository.Load(command.UserId);
            user.ChangeEmail(command.Email);
            user.ChangeLockStatus(command.IsLocked);
      //      user.ChangePassword(command.Password);
            user.ChangeRoleList(command.Roles);
            if (!string.IsNullOrEmpty(command.SupervisorId))
            {
                User supervisor = _repository.Load(command.SupervisorId);
                user.SetSupervisor(supervisor.CreateSupervisor());
            }
            else
            {
                user.ClearSupervisor();
            }
            var location = _locationalRepository.Load(command.LocationId);
            user.SetLocaton(location);
        }

        #endregion
    }
}
