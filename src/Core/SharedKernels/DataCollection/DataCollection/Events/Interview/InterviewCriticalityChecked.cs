using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview;

public class InterviewCriticalityChecked : InterviewActiveEvent
{
    public CriticalLevel CriticalLevel { get; }
    public Guid[] FailedCriticalRules { get; }
    public Identity[] UnansweredCriticalQuestions { get; }

    public InterviewCriticalityChecked(Guid userId, DateTimeOffset originDate, CriticalLevel criticalLevel, 
        Guid[] failedCriticalRules, Identity[] unansweredCriticalQuestions)
        : base(userId, originDate)
    {
        CriticalLevel = criticalLevel;
        FailedCriticalRules = failedCriticalRules;
        UnansweredCriticalQuestions = unansweredCriticalQuestions;
    }
}
