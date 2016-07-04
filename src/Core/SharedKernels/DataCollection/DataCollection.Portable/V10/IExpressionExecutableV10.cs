using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V9;

namespace WB.Core.SharedKernels.DataCollection.V10
{
    public interface IExpressionExecutableV10 : IExpressionExecutableV9
    {
        IExpressionExecutableV10 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV10>> getInstances);
        void SetParent(IExpressionExecutableV10 parentLevel);

        new IExpressionExecutableV10 GetParent();

        new IExpressionExecutableV10 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector,
            Identity[] rosterIdentityKey);

        IEnumerable<CategoricalOption> FilterOptionsForQuestion(Guid questionId, IEnumerable<CategoricalOption> options);

        decimal[] RosterVector { get; }

        List<Guid> LinkedQuestions { get; }

        void SetRostersRemover(Action<Identity[], Guid, decimal> removeRosterInstances);

        void SetStructuralChangesCollector(StructuralChanges structuralChanges);

        Guid[] GetRosterIdsThisScopeConsistOf();
        
        LinkedQuestionFilterResult ExecuteLinkedQuestionFilter(IExpressionExecutableV10 currentScope, Guid questionId);

        void RemoveAnswer(Guid questionId);
    }
}