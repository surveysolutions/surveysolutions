using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Services
{
    public class CompleteQuestionnaireUploaderService: ICompleteQuestionnaireUploaderService
    {
        private ICompleteQuestionnaireRepository _questionRepository;

        public CompleteQuestionnaireUploaderService(ICompleteQuestionnaireRepository questionRepository)
        {
            this._questionRepository = questionRepository;
        }
        public CompleteQuestionnaire AddCompleteAnswer(Questionnaire questionnaire,
                                 IEnumerable<CompleteAnswer> answers, UserLight user, SurveyStatus status)
        {
            CompleteQuestionnaire entity = new CompleteQuestionnaire(questionnaire, user, status);
            foreach (CompleteAnswer completeAnswer in answers)
            {

                entity.UpdateAnswer(completeAnswer, null);
            }
            _questionRepository.Add(entity);
            return entity;
        }
        public CompleteQuestionnaire CreateCompleteQuestionnaire(Questionnaire questionnaire,UserLight user, SurveyStatus status)
        {
            CompleteQuestionnaire entity = new CompleteQuestionnaire(questionnaire, user, status);
           
            _questionRepository.Add(entity);
            return entity;
        }

        public CompleteQuestionnaire UpdateCompleteAnswer(string id, Questionnaire questionnaire,
                                 IEnumerable<CompleteAnswer> answers)
        {
            CompleteQuestionnaire entity = _questionRepository.Load(id);
            if (entity.GetQuestionnaireTemplate().QuestionnaireId != questionnaire.QuestionnaireId)
                throw new InvalidOperationException(
                    "You can't attach different questionnaire to completed questionnaire on updating.");
            foreach (CompleteAnswer completeAnswer in answers)
            {

                entity.UpdateAnswer(completeAnswer, null);
            }
            _questionRepository.Add(entity);
            return entity;
        }
    }
}
