using System;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.Subscribers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;


namespace RavenQuestionnaire.Core.Services
{
    public class CompleteQuestionnaireUploaderService: ICompleteQuestionnaireUploaderService
    {
        private ICompleteQuestionnaireRepository _questionRepository;
        private IStatisticRepository _statisticsRepository;
        private ISubscriber subscriber;
        public CompleteQuestionnaireUploaderService(ICompleteQuestionnaireRepository questionRepository, IStatisticRepository statisticsRepository, ISubscriber subscriber)
        {
            this._questionRepository = questionRepository;
            this._statisticsRepository = statisticsRepository;
            this.subscriber = subscriber;
        }
        public CompleteQuestionnaire AddCompleteAnswer(string id, Guid questionKey, Guid? propagationKey, object answers)
        {
         //   PropagatableCompleteAnswer propagated = completeAnswers[0] as PropagatableCompleteAnswer;

            CompleteQuestionnaire entity = _questionRepository.Load(id);
            ICompleteGroup general = entity.GetInnerDocument();
            ICompleteQuestion question = FindQuestion(questionKey, propagationKey, general);
            question.SetAnswer(answers);
         //   entity.GetInnerDocument().QuestionHash[question].SetAnswer(answers);
            return entity;
        }

        #region update utilitie
        protected ICompleteQuestion FindQuestion(Guid questionKey, Guid? propagationKey, ICompleteGroup entity)
        {
            //PropagatableCompleteAnswer propagated = answer as PropagatableCompleteAnswer;

            var question = entity.FirstOrDefault<ICompleteQuestion>(q => q.PublicKey == questionKey);
            if (question == null)
                throw new ArgumentException("question wasn't found");
            if (!propagationKey.HasValue)
                return question;
            return entity.GetPropagatedQuestion(question.PublicKey, propagationKey.Value);
        }

        #endregion
        public CompleteQuestionnaire CreateCompleteQuestionnaire(Questionnaire questionnaire, Guid completeQuestionnaireGuid, UserLight user, SurveyStatus status)
        {
            CompleteQuestionnaire entity = new CompleteQuestionnaire(questionnaire,completeQuestionnaireGuid, user, status, this.subscriber);
           
            _questionRepository.Add(entity);
           
       //     this._statisticsRepository.Add(new CompleteQuestionnaireStatistics(entity.GetInnerDocument()));
            return entity;
        }

        public void DeleteCompleteQuestionnaire(string id)
        {
            var entity = _questionRepository.Load(id);
            if(entity!=null)
                this._questionRepository.Remove(entity);
            var statEntity = _statisticsRepository.Load(IdUtil.CreateStatisticId(IdUtil.ParseId(id)));
            if (statEntity != null)
                this._statisticsRepository.Remove(statEntity);
        }

        public void PropagateGroup(string id, Guid publicKey, Guid groupPublicKey)
        {
            CompleteQuestionnaire entity = _questionRepository.Load(id);
            var template = entity.Find<CompleteGroup>(groupPublicKey);
       
                var newGroup = new CompleteGroup(template, publicKey);
                entity.Add(newGroup, null);

              
             /*   return;
          }
            throw new InvalidOperationException("Group can't be added");*/
        }

        public void RemovePropagatedGroup(string id, Guid publicKey, Guid propagationKey)
        {
            CompleteQuestionnaire entity = _questionRepository.Load(id);
            //   entity.Remove(new PropagatableCompleteGroup(entity.Find<CompleteGroup>(command.GroupPublicKey)))

            entity.Remove(new CompleteGroup(entity.Find<CompleteGroup>(publicKey),
                                                        propagationKey));

           
        }

        public void AddComments(string id, Guid publicKey, Guid? propagationKey, string comments)
        {
            CompleteQuestionnaire entity = _questionRepository.Load(id);
            ICompleteGroup general = entity.GetInnerDocument();
            ICompleteQuestion question = FindQuestion(publicKey, propagationKey, general);
            question.SetComments(comments);
        }
    }
}
