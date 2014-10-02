using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public class DummyInterviewExpressionState : AbstractInterviewExpressionState {
        public override void AddRoster(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
        {
            throw new NotImplementedException();
        }

        public override void RemoveRoster(Guid rosterId, decimal[] rosterVector, decimal rosterInstanceId)
        {
            throw new NotImplementedException();
        }

        public override void UpdateNumericIntegerAnswer(Guid questionId, decimal[] rosterVector, long answer)
        {
            throw new NotImplementedException();
        }

        public override void UpdateNumericRealAnswer(Guid questionId, decimal[] rosterVector, double answer)
        {
            throw new NotImplementedException();
        }

        public override void UpdateDateAnswer(Guid questionId, decimal[] rosterVector, DateTime answer)
        {
            throw new NotImplementedException();
        }

        public override void UpdateTextAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            throw new NotImplementedException();
        }

        public override void UpdateQrBarcodeAnswer(Guid questionId, decimal[] rosterVector, string answer)
        {
            throw new NotImplementedException();
        }

        public override void UpdateSingleOptionAnswer(Guid questionId, decimal[] rosterVector, decimal answer)
        {
            throw new NotImplementedException();
        }

        public override void UpdateMultiOptionAnswer(Guid questionId, decimal[] rosterVector, decimal[] answer)
        {
            throw new NotImplementedException();
        }

        public override void UpdateGeoLocationAnswer(Guid questionId, decimal[] propagationVector, double latitude, double longitude, double accuracy,
            double altitude)
        {
            throw new NotImplementedException();
        }

        public override void UpdateTextListAnswer(Guid questionId, decimal[] propagationVector, Tuple<decimal, string>[] answers)
        {
            throw new NotImplementedException();
        }

        public override void UpdateLinkedSingleOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[] selectedPropagationVector)
        {
            throw new NotImplementedException();
        }

        public override void UpdateLinkedMultiOptionAnswer(Guid questionId, decimal[] propagationVector, decimal[][] selectedPropagationVectors)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<Guid, Guid[]> GetParentsMap()
        {
            throw new NotImplementedException();
        }

        public override IInterviewExpressionState Clone()
        {
            throw new NotImplementedException();
        }
    }
}