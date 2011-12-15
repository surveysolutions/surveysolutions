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
        public CompleteQuestionnaire AddCompleteAnswer(Entities.Questionnaire questionnaire,
                                 IEnumerable<CompleteAnswer> answers, string userId)
        {
            CompleteQuestionnaire entity = new CompleteQuestionnaire(questionnaire, userId);
            entity.UpdateAnswerList(answers);
            _questionRepository.Add(entity);
            return entity;
        }
        public CompleteQuestionnaire UpdateCompleteAnswer(string id, Entities.Questionnaire questionnaire,
                                 IEnumerable<CompleteAnswer> answers)
        {
            CompleteQuestionnaire entity = _questionRepository.Load(id);
            if (entity.GetQuestionnaireTemplate().QuestionnaireId != questionnaire.QuestionnaireId)
                throw new InvalidOperationException(
                    "You can't attach different questionnaire to completted questionnaire if updating it.");
            entity.UpdateAnswerList(answers);
            _questionRepository.Add(entity);
            return entity;
        }
    }
}
