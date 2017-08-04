using System;
using System.Collections.Generic;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.V10;

namespace WB.Tests.Abc.TestFactories
{
    internal class InterviewExpressionStateStub : AbstractInterviewExpressionStateV10, ILatestInterviewExpressionState
    {
        public override Dictionary<Guid, Guid[]> GetParentsMap() => new Dictionary<Guid, Guid[]>();
        protected override Guid GetQuestionnaireId() => Guid.Empty;
        protected override Guid[] GetParentRosterScopeIds(Guid rosterId) => new Guid[0];
        protected override bool HasParentScropeRosterId(Guid rosterId) => false;
        public override IInterviewExpressionState Clone() => this;
        ILatestInterviewExpressionState ILatestInterviewExpressionState.Clone() => this;

        //temp fix 
        public new bool AreLinkedQuestionsSupported() => false;
    }

    internal class InterviewExpressionStorageStub : IInterviewExpressionStorage
    {
        public void Initialize(IInterviewStateForExpressions state)
        {
        }

        public IInterviewLevel GetLevel(Identity rosterIdentity)
        {
            return new Mock<IInterviewLevel>().Object;
        }
    }
}

