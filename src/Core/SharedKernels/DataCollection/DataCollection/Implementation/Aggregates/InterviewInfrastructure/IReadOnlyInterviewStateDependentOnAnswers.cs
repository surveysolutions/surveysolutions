using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public interface IReadOnlyInterviewStateDependentOnAnswers
    {
        IReadOnlyInterviewStateDependentOnAnswers Amend(Func<Guid, RosterVector, IEnumerable<decimal>> getRosterInstanceIds);

        bool IsStaticTextDisabled(Identity group);
        bool IsGroupDisabled(Identity group);
        bool IsQuestionDisabled(Identity question);
        bool WasQuestionAnswered(Identity question);
        object GetAnswerSupportedInExpressions(Identity question);
        Tuple<decimal, string>[] GetTextListAnswer(Identity question);
        ReadOnlyCollection<decimal> GetRosterInstanceIds(Guid groupId, RosterVector outerRosterVector);
        IEnumerable<Tuple<Identity, RosterVector>> GetAllLinkedToQuestionSingleOptionAnswers(IQuestionnaire questionnaire);
        IEnumerable<Tuple<Identity, RosterVector[]>> GetAllLinkedMultipleOptionsAnswers(IQuestionnaire questionnaire);

        IEnumerable<Tuple<Identity, RosterVector>> GetAllLinkedToRosterSingleOptionAnswers(IQuestionnaire questionnaire);
        IEnumerable<Tuple<Identity, RosterVector[]>> GetAllLinkedToRosterMultipleOptionsAnswers(IQuestionnaire questionnaire);

        IReadOnlyCollection<RosterVector> GetOptionsForLinkedQuestion(Identity linkedQuestionIdentity);
    }
}