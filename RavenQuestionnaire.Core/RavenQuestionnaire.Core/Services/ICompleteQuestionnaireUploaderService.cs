using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Services
{
    public interface ICompleteQuestionnaireUploaderService
    {
        CompleteQuestionnaire AddCompleteAnswer(Questionnaire questionnaire, IEnumerable<CompleteAnswer> answers, UserLight user, SurveyStatus status);
        CompleteQuestionnaire CreateCompleteQuestionnaire(Questionnaire questionnaire, UserLight user, SurveyStatus status);
        void DeleteCompleteQuestionnaire(string id);
    }
}
