using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Portable;

namespace WB.Core.SharedKernels.DataCollection.ExpressionStorage
{
    public interface IInterviewStateForExpressions
    {
        int Version { get; }
        T GetAnswer<T>(Guid questionId, IEnumerable<int> rosterVector);

        AudioAnswerForConditions GetAudioAnswer(Guid questionId, IEnumerable<int> rosterVector);
        DateTime? GetDateTimeAnswer(Guid questionId, IEnumerable<int> rosterVector);
        TextListAnswerRow[] GetTextListAnswer(Guid questionId, IEnumerable<int> rosterVector);
        GeoLocation GetGeoLocationAnswer(Guid questionId, IEnumerable<int> rosterVector);
        RosterVector[] GetRosterVectorArrayAnswer(Guid questionId, IEnumerable<int> rosterVector);
        RosterVector GetRosterVectorAnswer(Guid questionId, IEnumerable<int> rosterVector);
        YesNoAndAnswersMissings GetYesNoAnswer(Guid questionId, IEnumerable<int> rosterVector);
        int[] GetIntArrayAnswer(Guid questionId, IEnumerable<int> rosterVector);
        double? GetDoubleAnswer(Guid questionId, IEnumerable<int> rosterVector);
        int? GetIntAnswer(Guid questionId, IEnumerable<int> rosterVector);
        string GetStringAnswer(Guid questionId, IEnumerable<int> rosterVector);

        T GetVariable<T>(Guid questionId, IEnumerable<int> rosterVector);

        IEnumerable<Identity> FindEntitiesFromSameOrDeeperLevel(Guid entityIdToSearch, Identity startingSearchPointIdentity);

        int GetRosterIndex(Identity rosterIdentity);

        string GetRosterTitle(Identity rosterIdentity);

        IInterviewPropertiesForExpressions Properties { get; }
    }

    public interface IInterviewPropertiesForExpressions
    {
        double Random { get; }

        Guid SupervisorId { get; }

        Guid InterviewerId { get; }

        string InterviewId { get; }
    }
}