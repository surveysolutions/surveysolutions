using System;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Services
{
    public interface ICompleteQuestionnaireUploaderService
    {
        CompleteQuestionnaire AddCompleteAnswer(string id, Guid questionKey, Guid? propagationKey,  object answers);
        CompleteQuestionnaire CreateCompleteQuestionnaire(Questionnaire questionnaire, Guid completeQuestionnaireGuid, UserLight user, SurveyStatus status);
        void DeleteCompleteQuestionnaire(string id);
        void PropagateGroup(string id, Guid publicKey, Guid groupPublicKey);
        void RemovePropagatedGroup(string id, Guid publicKey, Guid propagationKey);
        void AddComments(string id, Guid publicKey, Guid? propagationKey, string comments);
    }
}
