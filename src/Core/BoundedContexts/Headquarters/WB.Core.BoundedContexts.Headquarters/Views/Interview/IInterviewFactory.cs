using System;
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

        InterviewGpsAnswerWithTimeStamp[] GetGpsAnswersForInterviewer(Guid interviewerId);
        bool HasAnyGpsAnswerForInterviewer(Guid interviewerId);

        InterviewGpsInfo[] GetPrefilledGpsAnswers(Guid responsibleId,
            Guid? questionnaireId, long? questionnaireVersion, 
            double east, double north, double west, double south);
    }
}
