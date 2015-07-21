using System;

using WB.Core.SharedKernels.DataCollection.V2;

namespace WB.Core.SharedKernels.DataCollection
{
    public static class InterviewExpressionStateExtensions
    {
        public static void RemoveAnswer(this IInterviewExpressionStateV2 state, Guid id, decimal[] rosterVector)
        {
            state.UpdateSingleOptionAnswer(id, rosterVector, null);
            state.UpdateNumericIntegerAnswer(id, rosterVector, null);
            state.UpdateNumericRealAnswer(id, rosterVector, null);
            state.UpdateDateAnswer(id, rosterVector, null);
            state.UpdateMediaAnswer(id, rosterVector, null);
            state.UpdateTextAnswer(id, rosterVector, null);
            state.UpdateQrBarcodeAnswer(id, rosterVector, null);
            state.UpdateSingleOptionAnswer(id, rosterVector, null);
            state.UpdateMultiOptionAnswer(id, rosterVector, null);
            state.UpdateTextListAnswer(id, rosterVector, null);
            state.UpdateLinkedSingleOptionAnswer(id, rosterVector, null);
            state.UpdateLinkedMultiOptionAnswer(id, rosterVector, null);
        }
    }
}