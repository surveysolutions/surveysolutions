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

            foreach (CompleteAnswer completeAnswer in command.CompleteAnswers )
            {
                entity.UpdateAnswer(completeAnswer, null);
            }


            var status = _statusRepository.Load(IdUtil.CreateStatusId(command.StatusId));

            if (status != null)
                entity.SetStatus(new SurveyStatus(command.StatusId, status.GetInnerDocument().Title));

            var user = _userRepository.Load(IdUtil.CreateUserId(command.ResponsibleId));

            if (user != null)
                entity.SetResponsible(new UserLight() { Id = command.ResponsibleId, Name = user.GetInnerDocument().UserName });
            

        }
    }
}
