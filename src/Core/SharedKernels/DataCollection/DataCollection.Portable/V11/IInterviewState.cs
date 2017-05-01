using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.V11
{
    public interface IInterviewState
    {
        string GetTextAnswer(Guid questionId, IEnumerable<int> rosterVector);
        int? GetIntegerAnswer(Guid questionId, IEnumerable<int> rosterVector);
        double? GetDoubleAnswer(Guid questionId, IEnumerable<int> rosterVector);
        string GetQRBarcodeAnswer(Guid questionId, IEnumerable<int> rosterVector);
        YesNoAnswers GetYesNoAnswer(Guid questionId, IEnumerable<int> rosterVector);
        int[] GetMultiAnswer(Guid questionId, IEnumerable<int> rosterVector);
        int[] GetMultiLinkedToListAnswer(Guid questionId, IEnumerable<int> rosterVector);
        RosterVector[] GetMultiLinkedAnswer(Guid questionId, IEnumerable<int> rosterVector);
        DateTime? GetDateTimeAnswer(Guid questionId, IEnumerable<int> rosterVector);
        int? GetSingleAnswer(Guid questionId, IEnumerable<int> rosterVector);
        int? GetSingleLinkedToListAnswer(Guid questionId, IEnumerable<int> rosterVector);
        RosterVector GetSingleLinkedAnswer(Guid questionId, IEnumerable<int> rosterVector);
        ListAnswerRow[] GetTextListAnswer(Guid questionId, IEnumerable<int> rosterVector);
        GeoLocation GetGpsAnswer(Guid questionId, IEnumerable<int> rosterVector);
        string GetMultimediaAnswer(Guid questionId, IEnumerable<int> rosterVector);

        int GetRosterIndex(Identity rosterIdentity);

        string GetRosterTitle(Identity rosterIdentity);
    }
}