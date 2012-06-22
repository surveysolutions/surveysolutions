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
        CompleteQuestionnaire AddCompleteAnswer(string id, Guid questionKey, Guid? propagationKey,  object answers);
        CompleteQuestionnaire CreateCompleteQuestionnaire(Questionnaire questionnaire, Guid completeQuestionnaireGuid, UserLight user, SurveyStatus status);
        void DeleteCompleteQuestionnaire(string id);
        void PropagateGroup(string id, Guid publicKey, Guid groupPublicKey);
        void RemovePropagatedGroup(string id, Guid publicKey, Guid propagationKey);
    }
}
