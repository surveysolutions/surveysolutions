using RavenQuestionnaire.Core.Commands.User;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.User
{
    public class ChangeUserStatusHandler : ICommandHandler<ChangeUserStatusCommand>
    {
        private readonly IUserRepository _userRepository;

        public ChangeUserStatusHandler(IUserRepository repository)
        {
            _userRepository = repository;
        }

        #region Implementation of ICommandHandler<UpdateUserCommand>

        public void Handle(ChangeUserStatusCommand command)
        {
            Entities.User user = _userRepository.Load(command.UserId);
            user.ChangeLockStatus(command.IsLocked);
        }

        #endregion
    }
}