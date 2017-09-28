using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IInterviewFactory
    {
        Identity[] GetFlaggedQuestionIds(Guid interviewId);
        void SetFlagToQuestion(Guid interviewId, Identity questionIdentity, bool flagged);
        void RemoveInterview(Guid interviewId);

        void UpdateAnswer(Guid interviewId, Identity questionIdentity, object answer);
        void MakeEntitiesValid(Guid interviewId, Identity[] entityIds, EntityType entityType);
        void MakeEntitiesInvalid(Guid interviewId, IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> entityIds, EntityType entityType);
        void EnableEntities(Guid interviewId, Identity[] entityIds, EntityType entityType, bool isEnabled);
        void UpdateVariables(Guid interviewId, ChangedVariable[] variables);
        void MarkQuestionsAsReadOnly(Guid interviewId, Identity[] questionIds);
        void AddRosters(Guid interviewId, Identity[] rosterIds);
        void RemoveRosters(Guid interviewId, Identity[] rosterIds);
        void RemoveAnswers(Guid interviewId, Identity[] questionIds);
        
        InterviewData GetInterviewData(Guid interviewId);
        InterviewStringAnswer[] GetAllMultimediaAnswers(Guid[] multimediaQuestionIds);
        InterviewStringAnswer[] GetAllAudioAnswers();
        Guid[] GetAnsweredGpsQuestionIdsByQuestionnaire(QuestionnaireIdentity questionnaireIdentity);

        InterviewGpsAnswer[] GetGpsAnswersByQuestionIdAndQuestionnaire(QuestionnaireIdentity questionnaireIdentity,
            Guid gpsQuestionId, int maxAnswersCount, double northEastCornerLatitude,
            double southWestCornerLatitude, double northEastCornerLongtitude, double southWestCornerLongtitude);

        string[] GetQuestionnairesWithAnsweredGpsQuestions();
    }
}