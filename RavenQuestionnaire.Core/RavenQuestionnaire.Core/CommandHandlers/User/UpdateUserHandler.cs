using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.User
{
    public class UpdateUserHandler : ICommandHandler<UpdateUserCommand>
    {
        private IUserRepository _userRepository;
        private ILocationRepository _locationalRepository;
        public UpdateUserHandler(IUserRepository repository, ILocationRepository locationRepository)
        {
            _userRepository = repository;
            _locationalRepository = locationRepository;
        }

        #region Implementation of ICommandHandler<UpdateUserCommand>

        public void Handle(UpdateUserCommand command)
        {
            Entities.User user = _userRepository.Load(command.UserId);
            user.ChangeEmail(command.Email);
            user.ChangeLockStatus(command.IsLocked);
      //      user.ChangePassword(command.Password);
            user.ChangeRoleList(command.Roles);
            if (!string.IsNullOrEmpty(command.SupervisorId))
            {
                Entities.User supervisor = _userRepository.Load(command.SupervisorId);
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
