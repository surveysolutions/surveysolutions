using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RavenQuestionnaire.Core.Commands.Statistics;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.Subscribers;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Services
{
    public class CompleteQuestionnaireUploaderService: ICompleteQuestionnaireUploaderService
    {
        private ICompleteQuestionnaireRepository _questionRepository;
        private IStatisticRepository _statisticsRepository;
        private ICommandInvokerAsync _asyncInvocker;
        private ISubscriber subscriber;
        public CompleteQuestionnaireUploaderService(ICompleteQuestionnaireRepository questionRepository, IStatisticRepository statisticsRepository, ICommandInvokerAsync asyncInvocker, ISubscriber subscriber)
        {
            this._questionRepository = questionRepository;
            this._statisticsRepository = statisticsRepository;
            this._asyncInvocker = asyncInvocker;
            this.subscriber = subscriber;
        }
        public CompleteQuestionnaire AddCompleteAnswer(string id, CompleteAnswer[] completeAnswers)
        {
            PropagatableCompleteAnswer propagated = completeAnswers[0] as PropagatableCompleteAnswer;

            CompleteQuestionnaire entity = _questionRepository.Load(id);
            ICompleteGroup general = entity.GetInnerDocument();
            ICompleteQuestion question;
            question = propagated == null ? FindQuestion(completeAnswers[0].PublicKey, null, general) : FindQuestion(completeAnswers[0].PublicKey, propagated.PropogationPublicKey, general);
            if (question == null)
                throw new ArgumentException("question wasn't found");

            foreach (var completeAnswer in completeAnswers)
            {
                if (completeAnswer.Selected)
                    entity.Add(completeAnswer, question.PublicKey);
                else
                    entity.Remove(completeAnswer);
            }
            ExecuteConditions(question, general);


            var command = new GenerateQuestionnaireStatisticCommand(entity, null);
            _asyncInvocker.Execute(command);
            return entity;
        }

        #region update utilitie
        protected void ExecuteConditions(ICompleteQuestion question, ICompleteGroup entity)
        {
            PropagatableCompleteQuestion propagated = question as PropagatableCompleteQuestion;
            IEnumerable<ICompleteQuestion> triggeres;
            if (propagated == null)
            {
                triggeres =
               entity.Find<ICompleteQuestion>(
                   g => g.Triggers.Count(gp => gp.Equals(question.PublicKey)) > 0).ToList();
            }
            else
            {
                triggeres =
                    entity.GetPropagatedGroupsByKey(propagated.PropogationPublicKey).SelectMany(g => g.Find<ICompleteQuestion>(
                        q => q.Triggers.Count(gp => gp.Equals(question.PublicKey)) > 0)).ToList();
            }
            var executor = new CompleteQuestionnaireConditionExecutor(entity);
            foreach (ICompleteQuestion completeQuestion in triggeres)
            {
                bool previousState = completeQuestion.Enabled;
                completeQuestion.Enabled = executor.Execute(completeQuestion);
                if (!completeQuestion.Enabled)
                    entity.Remove(completeQuestion);
                if (previousState != completeQuestion.Enabled)
                {
                    ExecuteConditions(completeQuestion, entity);
                }
            }
        }
        protected ICompleteQuestion FindQuestion(Guid answerKey, Guid? propagationKey, ICompleteGroup entity)
        {
            //PropagatableCompleteAnswer propagated = answer as PropagatableCompleteAnswer;

            var question = entity.FirstOrDefault<ICompleteQuestion>(q => q.Children.Any(a => a.PublicKey == answerKey));
            if (!propagationKey.HasValue)
                return question;
            return entity.GetPropagatedQuestion(question.PublicKey, propagationKey.Value);
        }

        #endregion
        public CompleteQuestionnaire CreateCompleteQuestionnaire(Questionnaire questionnaire,UserLight user, SurveyStatus status)
        {
            CompleteQuestionnaire entity = new CompleteQuestionnaire(questionnaire, user, status, this.subscriber);
           
            _questionRepository.Add(entity);
            var command = new GenerateQuestionnaireStatisticCommand(entity, null);

            _asyncInvocker.Execute(command);
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

        public Guid PropagateGroup(string id, Guid publicKey)
        {
            CompleteQuestionnaire entity = _questionRepository.Load(id);
            var template = entity.Find<CompleteGroup>(publicKey);
            bool isCondition = false;
            var executor = new CompleteQuestionnaireConditionExecutor(entity.GetInnerDocument());
            foreach (CompleteQuestion completeQuestion in template.GetAllQuestions<ICompleteQuestion>())
            {
                if (executor.Execute(completeQuestion))
                {
                    isCondition = true;
                    completeQuestion.Enabled = true;
                }
                else
                {
                    completeQuestion.Enabled = false;
                }
            }
            if (isCondition)
            {
                var propagationKey = Guid.NewGuid();
                var newGroup = new PropagatableCompleteGroup(template, propagationKey);
                entity.Add(newGroup, null);

                var command = new GenerateQuestionnaireStatisticCommand(entity, null);

                _asyncInvocker.Execute(command);
                return propagationKey;
            }
            throw new InvalidOperationException("Group can't be added");
        }

        public void RemovePropagatedGroup(string id, Guid publicKey, Guid propagationKey)
        {
            CompleteQuestionnaire entity = _questionRepository.Load(id);
            //   entity.Remove(new PropagatableCompleteGroup(entity.Find<CompleteGroup>(command.GroupPublicKey)))

            entity.Remove(new PropagatableCompleteGroup(entity.Find<CompleteGroup>(publicKey),
                                                        propagationKey));

            var command = new GenerateQuestionnaireStatisticCommand(entity, null);

            _asyncInvocker.Execute(command);
        }
    }
}
