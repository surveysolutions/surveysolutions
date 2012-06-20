using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Entities.Statistics;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.EventSubscribers
{
    public class GenerateQuestionnaireStatisticEventHandler :
        IEventSubscriber<UpdateAnswerInCompleteQuestionnaireCommand>,
        IEventSubscriber<UpdateCompleteQuestionnaireCommand>, IEventSubscriber<CreateNewCompleteQuestionnaireCommand>,
        IEventSubscriber<PropagateGroupCommand>, IEventSubscriber<DeletePropagatedGroupCommand>,
        IEventSubscriber<ValidateGroupCommand>
    {
        private IStatisticRepository statisticRepository;
        private ICompleteQuestionnaireRepository questionnaireRepository;

        public GenerateQuestionnaireStatisticEventHandler(IStatisticRepository repository,
                                                          ICompleteQuestionnaireRepository questionnaireRepository)
        {
            this.statisticRepository = repository;
            this.questionnaireRepository = questionnaireRepository;
        }

        #region Implementation of ICommandHandler<in GenerateQuestionnaireStatisticCommand>

        protected void Handle(string questionnaireId)
        {
            //    
            CompleteQuestionnaireStatistics statistics =
                this.statisticRepository.Load(IdUtil.CreateStatisticId(IdUtil.ParseId(questionnaireId)));
            var entity =
                this.questionnaireRepository.Load(questionnaireId);

            if (statistics == null)
            {
                statistics = new CompleteQuestionnaireStatistics(entity.GetInnerDocument());
                statisticRepository.Add(statistics);
            }
            else
            {
                statistics.UpdateStatistic(entity.GetInnerDocument());
            }
        }

        #endregion

        #region Implementation of IEventSubscriber<in UpdateAnswerInCompleteQuestionnaireCommand>

        public void Invoke(UpdateAnswerInCompleteQuestionnaireCommand command)
        {
            Handle(command.CompleteQuestionnaireId);
        }

        #endregion

        #region Implementation of IEventSubscriber<in UpdateCompleteQuestionnaireCommand>

        public void Invoke(UpdateCompleteQuestionnaireCommand command)
        {
            Handle(command.CompleteQuestionnaireId);
        }

        #endregion

        #region Implementation of IEventSubscriber<in CreateNewCompleteQuestionnaireCommand>

        public void Invoke(CreateNewCompleteQuestionnaireCommand command)
        {
            Handle(command.CompleteQuestionnaireGuid.ToString());
        }

        #endregion

        #region Implementation of IEventSubscriber<in PropagateGroupCommand>

        public void Invoke(PropagateGroupCommand command)
        {
            Handle(command.CompleteQuestionnaireId);
        }

        #endregion

        #region Implementation of IEventSubscriber<in DeletePropagatedGroupCommand>

        public void Invoke(DeletePropagatedGroupCommand command)
        {
            Handle(command.CompleteQuestionnaireId);
        }

        #endregion

        #region Implementation of IEventSubscriber<in ValidateGroupCommand>

        public void Invoke(ValidateGroupCommand command)
        {
            Handle(command.QuestionnaireId);
        }

        #endregion
    }
}
