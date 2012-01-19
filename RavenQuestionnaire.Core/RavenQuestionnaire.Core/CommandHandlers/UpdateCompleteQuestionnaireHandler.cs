using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.CommandHandlers
{
    public class UpdateCompleteQuestionnaireHandler : ICommandHandler<UpdateCompleteQuestionnaireCommand>
    {
        private ICompleteQuestionnaireRepository _questionnaireRepository;
        private IStatusRepository _statusRepository;
        private IUserRepository _userRepository;

        public UpdateCompleteQuestionnaireHandler(ICompleteQuestionnaireRepository questionnaireRepository, 
            IStatusRepository statusRepository,
            IUserRepository userRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._statusRepository = statusRepository;
            this._userRepository = userRepository;

        }

        public void Handle(UpdateCompleteQuestionnaireCommand command)
        {
            CompleteQuestionnaire entity = _questionnaireRepository.Load(command.CompleteQuestionnaireId);

            
            var status = _statusRepository.Load(IdUtil.CreateStatusId(command.Status.Id));

            if (status != null)
                entity.SetStatus(command.Status);
            //what to do if status is not present
            var user = _userRepository.Load(IdUtil.CreateUserId(command.Responsible.Id));

            if (user != null)
                entity.SetResponsible(command.Responsible);
            //what to do if user is not present

        }
    }
}
