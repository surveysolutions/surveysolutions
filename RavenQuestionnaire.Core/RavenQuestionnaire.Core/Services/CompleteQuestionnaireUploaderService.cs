using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Services
{
    public class CompleteQuestionnaireUploaderService: ICompleteQuestionnaireUploaderService
    {
        private ICompleteQuestionnaireRepository _questionRepository;
        private IStatisticRepository _statisticsRepository;
        public CompleteQuestionnaireUploaderService(ICompleteQuestionnaireRepository questionRepository, IStatisticRepository statisticsRepository)
        {
            this._questionRepository = questionRepository;
            this._statisticsRepository = statisticsRepository;
        }
        public CompleteQuestionnaire AddCompleteAnswer(Questionnaire questionnaire,
                                 IEnumerable<CompleteAnswer> answers, UserLight user, SurveyStatus status)
        {
            CompleteQuestionnaire entity = new CompleteQuestionnaire(questionnaire, user, status);
            foreach (CompleteAnswer completeAnswer in answers)
            {
             //   entity.Remove<Question>(completeAnswer.QuestionPublicKey);
                entity.Add(completeAnswer, completeAnswer.QuestionPublicKey);
               // entity.ChangeAnswer(completeAnswer);
            }
            _questionRepository.Add(entity);
            return entity;
        }
        public CompleteQuestionnaire CreateCompleteQuestionnaire(Questionnaire questionnaire,UserLight user, SurveyStatus status)
        {
            CompleteQuestionnaire entity = new CompleteQuestionnaire(questionnaire, user, status);
           
            _questionRepository.Add(entity);
            this._statisticsRepository.Add(new CompleteQuestionnaireStatistics(entity.GetInnerDocument()));
            return entity;
        }

        public void DeleteCompleteQuestionnaire(string id)
        {
            var entity = _questionRepository.Load(id);
            this._questionRepository.Remove(entity);
            var statEntity = _statisticsRepository.Load(IdUtil.CreateStatisticId(IdUtil.ParseId(id)));
            if (statEntity != null)
                this._statisticsRepository.Remove(statEntity);
        }
    }
}
