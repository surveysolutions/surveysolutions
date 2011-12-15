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
        CompleteQuestionnaire AddCompleteAnswer(Questionnaire questionnaire, IEnumerable<CompleteAnswer> answers, string userId);

        CompleteQuestionnaire UpdateCompleteAnswer(string id, Entities.Questionnaire questionnaire,
                                                   IEnumerable<CompleteAnswer> answers);
    }
}
