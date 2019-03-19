using System;
using System.Collections.Generic;
using Supercluster;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public interface IInterviewFactory
    {
        Identity[] GetFlaggedQuestionIds(Guid interviewId);
        void SetFlagToQuestion(Guid interviewId, Identity questionIdentity, bool flagged);
        void RemoveInterview(Guid interviewId);

        InterviewGpsAnswer[] GetGpsAnswers(Guid questionnaireId, long? questionnaireVersion, 
            string gpsQuestionVariableName, int? maxAnswersCount, Guid? supervisorId);

        IEnumerable<InterviewEntity> GetInterviewEntities(IEnumerable<Guid> interviews, Guid[] entityIds = null);
        List<InterviewEntity> GetInterviewEntities(Guid interviewId);
        void Save(InterviewState interviewState);
        InterviewGpsAnswerWithTimeStamp[] GetGpsAnswersForInterviewer(Guid interviewerId);
        bool HasAnyGpsAnswerForInterviewer(Guid interviewerId);
    }
}
