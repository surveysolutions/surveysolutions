using System;
using RavenQuestionnaire.Core.Commands.Status;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.CommandHandlers.Status
{
    public class CreateNewStatusHandler: ICommandHandler<CreateNewStatusCommand>
    {
        private readonly IStatusRepository _statusRepository;

        public CreateNewStatusHandler(IStatusRepository repository)
        {
            this._statusRepository = repository;
        }

        public void Handle(CreateNewStatusCommand command)
        {
            string statusId = IdUtil.CreateStatusId(command.StatusId);

            var status = _statusRepository.Load(statusId);
            if (status == null)
                throw new Exception("No status found " + statusId);

            var item = new StatusItem()
                           {
                               Title = command.Title,
                               IsInitial = command.IsInitial
                           }; 

            status.GetInnerDocument().Statuses.Add(item); 
        }
    }
}
