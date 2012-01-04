using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Services
{
    public interface ICompleteQuestionnaireUploaderService
    {
        CompleteQuestionnaire AddCompleteAnswer(Questionnaire questionnaire, IEnumerable<CompleteAnswer> answers, UserLight user, SurveyStatus status);

        CompleteQuestionnaire UpdateCompleteAnswer(string id, Questionnaire questionnaire,
                                                   IEnumerable<CompleteAnswer> answers);
        CompleteQuestionnaire CreateCompleteQuestionnaire(Questionnaire questionnaire, UserLight user, SurveyStatus status);
    }
}
