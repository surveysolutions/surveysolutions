using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IInterviewFactory
    {
        Identity[] GetQuestionsWithFlagBySectionId(QuestionnaireIdentity questionnaireId, Guid interviewId, Identity sectionId);
        Identity[] GetFlaggedQuestionIds(Guid interviewId);
        void SetFlagToQuestion(Guid interviewId, Identity questionIdentity, bool flagged);
        void RemoveInterview(Guid interviewId);

        InterviewStringAnswer[] GetMultimediaAnswersByQuestionnaire(QuestionnaireIdentity questionnaireIdentity);

        InterviewStringAnswer[] GetAudioAnswersByQuestionnaire(QuestionnaireIdentity questionnaireIdentity);
        Guid[] GetAnsweredGpsQuestionIdsByQuestionnaire(QuestionnaireIdentity questionnaireIdentity);

        InterviewGpsAnswer[] GetGpsAnswersByQuestionIdAndQuestionnaire(QuestionnaireIdentity questionnaireIdentity,
            Guid gpsQuestionId, int maxAnswersCount, double northEastCornerLatitude,
            double southWestCornerLatitude, double northEastCornerLongtitude, double southWestCornerLongtitude);

        string[] GetQuestionnairesWithAnsweredGpsQuestions();

        IEnumerable<InterviewEntity> GetInterviewEntities(IEnumerable<Guid> interviews);
        List<InterviewEntity> GetInterviewEntities(Guid interviewId);
        Dictionary<string, InterviewLevel> GetInterviewDataLevels(IQuestionnaire questionnaire, List<InterviewEntity> interviewEntities);
        void Save(InterviewState interviewState);
    }
}
