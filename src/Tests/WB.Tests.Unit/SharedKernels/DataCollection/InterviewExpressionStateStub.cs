using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    internal class InterviewExpressionStateStub : AbstractInterviewExpressionStateV6, IInterviewExpressionStateV7
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

        public LinkedQuestionOptionsChanges ProcessLinkedQuestionFilters()
        {
            throw new NotImplementedException();
        }

        public bool AreLinkedQuestionsSupported()
        {
            return false;
        }

        IInterviewExpressionStateV7 IInterviewExpressionStateV7.Clone()
        {
            return this;
        }
    }
}

