using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview;

public class InterviewCriticalityChecked : InterviewActiveEvent
{
    public CriticalLevel CriticalLevel { get; }
    public Guid[] FailCriticalRules { get; }
    public Identity[] UnansweredCriticalQuestions { get; }

    public InterviewCriticalityChecked(Guid userId, DateTimeOffset originDate, CriticalLevel criticalLevel, 
        Guid[] failCriticalRules, Identity[] unansweredCriticalQuestions)
        : base(userId, originDate)
    {
        CriticalLevel = criticalLevel;
        FailCriticalRules = failCriticalRules;
        UnansweredCriticalQuestions = unansweredCriticalQuestions;
    }
}
