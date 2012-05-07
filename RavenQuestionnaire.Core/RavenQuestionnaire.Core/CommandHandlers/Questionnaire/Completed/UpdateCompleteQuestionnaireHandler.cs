using System.Linq;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed
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

            if (entity != null)
            {
                var status = _statusRepository.Load(command.StatusHolderId);
                if (status != null)
                {
                    var newStatus = status.GetInnerDocument().Statuses.FirstOrDefault(x => x.PublicKey == command.Status);
                    if (newStatus != null)
                        entity.SetStatus(new SurveyStatus(newStatus.PublicKey, newStatus.Title));
                }
                //what to do if status is not present

                //do not change responsible if incoming value is null
                if (command.Responsible != null)
                {
                    var user = _userRepository.Load(command.Responsible);

                    if (user != null)
                        entity.SetResponsible(new UserLight(user.GetInnerDocument().Id, user.GetInnerDocument().UserName));
                    //what to do if user is not present?
                }
            }
        }
    }
}
