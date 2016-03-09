using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.V6;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    internal class InterviewExpressionStateStub : AbstractInterviewExpressionStateV6, IInterviewExpressionStateV6
    {
        public override Dictionary<Guid, Guid[]> GetParentsMap()
        {
            return new Dictionary<Guid, Guid[]>();
        }

        protected override Guid GetQuestionnaireId()
        {
            return Guid.Empty;
        }

        protected override Guid[] GetParentRosterScopeIds(Guid rosterId)
        {
            return new Guid[0];
        }

        protected override bool HasParentScropeRosterId(Guid rosterId)
        {
            return false;
        }

        public override IInterviewExpressionState Clone()
        {
            return this;
        }
    }
}

