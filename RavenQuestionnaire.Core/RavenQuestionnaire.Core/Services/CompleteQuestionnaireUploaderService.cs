using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
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
            return entity;
        }
    }
}
