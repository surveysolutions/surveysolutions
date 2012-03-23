using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.User
{
    public class CreateNewUserHandler:ICommandHandler<CreateNewUserCommand>
    {
        private IUserRepository _repository;
        private ILocationRepository _locationalRepository;
        public CreateNewUserHandler(IUserRepository repository, ILocationRepository locationRepository)
        {
            this._repository = repository;
            this._locationalRepository = locationRepository;
        }

        #region Implementation of ICommandHandler<CreateNewUserCommand>

        public void Handle(CreateNewUserCommand command)
        {
            Entities.User newUser = new Entities.User(command.UserName, command.Password, command.Email, command.Role,
                                         command.IsLocked);
            if (!string.IsNullOrEmpty(command.SupervisorId))
            {
                Entities.User supervisor = _repository.Load(command.SupervisorId);
                newUser.SetSupervisor(supervisor.CreateSupervisor());
            }
            var location = _locationalRepository.Load(command.LocationId);
            newUser.SetLocaton(location);
            _repository.Add(newUser);
        }

        #endregion
    }
}
