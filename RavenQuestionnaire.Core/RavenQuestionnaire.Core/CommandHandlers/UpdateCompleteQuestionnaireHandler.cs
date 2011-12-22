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

        public UpdateCompleteQuestionnaireHandler(ICompleteQuestionnaireRepository questionnaireRepository, IStatusRepository statusRepository)
        {
            this._questionnaireRepository = questionnaireRepository;
            this._statusRepository = statusRepository;
        }

        public void Handle(UpdateCompleteQuestionnaireCommand command)
        {
            CompleteQuestionnaire entity = _questionnaireRepository.Load(command.CompleteQuestionnaireId);
            entity.UpdateAnswerList(command.CompleteAnswers);

            var status = _statusRepository.Load(IdUtil.CreateStatusId(command.StatusId));
            entity.SetStatus(new SurveyStatus(command.StatusId, status.GetInnerDocument().Title));
            
        }
    }
}
