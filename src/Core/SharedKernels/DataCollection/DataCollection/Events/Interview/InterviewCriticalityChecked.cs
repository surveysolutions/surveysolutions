using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview;

public class InterviewCriticalityChecked : InterviewActiveEvent
{
    public CriticalityLevel CriticalityLevel { get; }
    public Guid[] FailedCriticalRules { get; }
    public Identity[] UnansweredCriticalQuestions { get; }

    public InterviewCriticalityChecked(Guid userId, DateTimeOffset originDate, CriticalityLevel criticalityLevel, 
        Guid[] failedCriticalRules, Identity[] unansweredCriticalQuestions)
        : base(userId, originDate)
    {
        CriticalityLevel = criticalityLevel;
        FailedCriticalRules = failedCriticalRules;
        UnansweredCriticalQuestions = unansweredCriticalQuestions;
    }
}
