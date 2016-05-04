using System;

namespace WB.Core.SharedKernels.DataCollection.ValueObjects.Interview
{
    [Flags]
    public enum QuestionState
    {
        Valid = 8,
        Enabled = 1,
        Flagged = 2,
        Answered = 4
    }
}