using System;
using System.Collections.Generic;
using System.Globalization;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.Enumerator.Aggregates
{
    public interface IStatefulInterview
    {
        QuestionnaireIdentity QuestionnaireIdentity { get; }
        string QuestionnaireId { get; }
        InterviewStatus Status { get; }

        Guid Id { get; }
        string InterviewerCompleteComment { get; }
        string SupervisorRejectComment { get; }
        
        string GetAnswerAsString(Identity questionIdentity, CultureInfo cultureInfo = null);

        string Language { get; }

        bool HasErrors { get; }
        bool IsCompleted { get; }
        bool CreatedOnClient { get; }

        InterviewTreeRoster GetRoster(Identity identity);

        InterviewTreeGpsQuestion GetGpsQuestion(Identity identity);

        InterviewTreeDateTimeQuestion GetDateTimeQuestion(Identity identity);

        InterviewTreeMultimediaQuestion GetMultimediaQuestion(Identity identity);

        InterviewTreeQRBarcodeQuestion GetQRBarcodeQuestion(Identity identity);

        InterviewTreeTextListQuestion GetTextListQuestion(Identity identity);

        InterviewTreeSingleLinkedToRosterQuestion GetLinkedSingleOptionQuestion(Identity identity);

        InterviewTreeMultiOptionQuestion GetMultiOptionQuestion(Identity identity);

        InterviewTreeMultiLinkedToRosterQuestion GetLinkedMultiOptionQuestion(Identity identity);

        InterviewTreeIntegerQuestion GetIntegerQuestion(Identity identity);

        InterviewTreeDoubleQuestion GetDoubleQuestion(Identity identity);

        InterviewTreeTextQuestion GetTextQuestion(Identity identity);

        InterviewTreeSingleOptionQuestion GetSingleOptionQuestion(Identity identity);

        InterviewTreeYesNoQuestion GetYesNoQuestion(Identity identity);

        InterviewTreeMultiOptionLinkedToListQuestion GetMultiOptionLinkedToListQuestion(Identity identity);

        InterviewTreeSingleOptionLinkedToListQuestion GetSingleOptionLinkedToListQuestion(Identity identity);

        bool HasGroup(Identity group);

        bool IsValid(Identity identity);

        IEnumerable<string> GetFailedValidationMessages(Identity questionOrStaticTextId);

        bool IsEnabled(Identity entityIdentity);

        bool WasAnswered(Identity entityIdentity);

        IEnumerable<AnswerComment> GetQuestionComments(Identity entityIdentity);

        string GetRosterTitle(Identity rosterIdentity);

        string GetTitleText(Identity entityIdentity);

        IEnumerable<string> GetParentRosterTitlesWithoutLast(Identity questionIdentity);

        IEnumerable<string> GetParentRosterTitlesWithoutLastForRoster(Identity rosterIdentity);

        int CountEnabledQuestions(Identity group);

        int GetGroupsInGroupCount(Identity group);

        int CountEnabledAnsweredQuestions(Identity group);

        int CountEnabledInvalidQuestionsAndStaticTexts(Identity group);

        bool HasEnabledInvalidQuestionsAndStaticTexts(Identity group);

        bool HasUnansweredQuestions(Identity group);

        Identity GetParentGroup(Identity groupOrQuestion);

        IEnumerable<Identity> GetChildQuestions(Identity groupIdentity);
        
        IEnumerable<Identity> GetEnabledSubgroups(Identity group);

        int CountActiveAnsweredQuestionsInInterview();

        int CountActiveQuestionsInInterview();

        int CountInvalidEntitiesInInterview();
        
        object GetVariableValueByOrDeeperRosterLevel(Guid variableId, RosterVector variableRosterVector);

        IEnumerable<Identity> GetInvalidEntitiesInInterview();

        List<CategoricalOption> GetTopFilteredOptionsForQuestion(Identity question, int? parentQuestionValue, string filter, int sliceSize);

        CategoricalOption GetOptionForQuestionWithoutFilter(Identity question, int value, int? parentQuestionValue = null);

        CategoricalOption GetOptionForQuestionWithFilter(Identity question, string value, int? parentQuestionValue = null);

        int CountCommentedQuestions();

        IEnumerable<Identity> GetCommentedBySupervisorQuestionsInInterview();

        string GetLastSupervisorComment();

        IReadOnlyList<Identity> GetRosterInstances(Identity parentIdentity, Guid rosterId);

        InterviewTreeQuestion FindQuestionInQuestionBranch(Guid entityId, Identity questionIdentity);

        string GetLinkedOptionTitle(Identity linkedQuestionIdentity, RosterVector option);
    }
}