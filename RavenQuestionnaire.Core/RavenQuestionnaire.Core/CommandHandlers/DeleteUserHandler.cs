using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class DeleteUserHandler : ICommandHandler<DeleteUserCommand>
    {
        private IUserRepository _repository;
        public DeleteUserHandler(IUserRepository repository)
        {

            _repository = repository;
        }

        #region Implementation of ICommandHandler<DeleteUserCommand>

        public void Handle(DeleteUserCommand command)
        {
            var entity = _repository.Load(command.UserId);
            entity.DeleteUser();
        }

        #endregion
    }
}
