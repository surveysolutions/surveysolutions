using System;
using System.Collections.Generic;
using System.Globalization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Aggregates
{
    public interface IStatefulInterview
    {
        DateTime? StartedDate { get; }
        DateTime? CompletedDate { get; }
        QuestionnaireIdentity QuestionnaireIdentity { get; }
        string QuestionnaireId { get; }
        InterviewStatus Status { get; }
        bool IsDeleted { get; }
        Guid Id { get; }
        string InterviewerCompleteComment { get; }
        string SupervisorRejectComment { get; }
        
        string GetAnswerAsString(Identity questionIdentity, CultureInfo cultureInfo = null);

        string Language { get; }

        bool HasErrors { get; }
        bool IsCompleted { get; }

        bool HasEditableIdentifyingQuestions { get; }

        bool ReceivedByInterviewer { get; }

        InterviewTreeGroup GetGroup(Identity identity);
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

        InterviewTreeAreaQuestion GetAreaQuestion(Identity identity);

        InterviewTreeAudioQuestion GetAudioQuestion(Identity identity);

        InterviewTreeQuestion GetQuestion(Identity identity);

        InterviewTreeStaticText GetStaticText(Identity identity);

        bool HasGroup(Identity group);

        bool IsEntityValid(Identity identity);
        bool IsEntityPlausible(Identity identity);

        IEnumerable<string> GetFailedValidationMessages(Identity questionOrStaticTextId, string defaltErrorMessageFallback);

        IEnumerable<string> GetFailedWarningMessages(Identity identity, string defaultText);

        bool IsEnabled(Identity entityIdentity);

        bool WasAnswered(Identity entityIdentity);

        IEnumerable<AnswerComment> GetQuestionComments(Identity entityIdentity);

        string GetRosterTitle(Identity rosterIdentity);

        string GetTitleText(Identity entityIdentity);
        string GetBrowserReadyTitleHtml(Identity entityIdentity);

        string GetBrowserReadyInstructionsHtml(Identity entityIdentity);

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

        IEnumerable<InterviewTreeGroup> GetAllEnabledGroupsAndRosters();

        IEnumerable<InterviewTreeGroup> GetAllGroupsAndRosters();

        InterviewTreeSection FirstSection { get; }

        Guid CurrentResponsibleId { get; }

        IEnumerable<InterviewTreeSection> GetEnabledSections();

        int CountActiveAnsweredQuestionsInInterview();
        int CountActiveQuestionsInInterview();
        int CountInvalidEntitiesInInterview();

        int CountActiveAnsweredQuestionsInInterviewForSupervisor();
        int CountActiveQuestionsInInterviewForSupervisor();
        int CountInvalidEntitiesInInterviewForSupervisor();

        int CountAllEnabledQuestions();
        int CountAllEnabledAnsweredQuestions();
        int CountAllInvalidEntities();

        object GetVariableValueByOrDeeperRosterLevel(Guid variableId, RosterVector variableRosterVector);

        IEnumerable<Identity> GetInvalidEntitiesInInterview();

        bool IsFirstEntityBeforeSecond(Identity first, Identity second);

        List<CategoricalOption> GetTopFilteredOptionsForQuestion(Identity question, int? parentQuestionValue, string filter, int sliceSize);

        CategoricalOption GetOptionForQuestionWithoutFilter(Identity question, int value, int? parentQuestionValue = null);

        CategoricalOption GetOptionForQuestionWithFilter(Identity question, string value, int? parentQuestionValue = null);

        IEnumerable<Identity> GetCommentedBySupervisorQuestionsVisibledToInterviewer();

        IEnumerable<Identity> GetCommentedBySupervisorAllQuestions();

        IEnumerable<Identity> GetAllCommentedEnabledQuestions();

        string GetLastSupervisorComment();

        IReadOnlyList<Identity> GetRosterInstances(Identity parentIdentity, Guid rosterId);

        InterviewTreeQuestion FindQuestionInQuestionBranch(Guid entityId, Identity questionIdentity);

        IEnumerable<Identity> FindQuestionsFromSameOrDeeperLevel(Guid entityId, Identity questionIdentity);

        bool IsQuestionPrefilled(Identity entityIdentity);

        string GetLinkedOptionTitle(Identity linkedQuestionIdentity, RosterVector option);
        
        IEnumerable<Identity> GetUnderlyingInterviewerEntities(Identity sectionId = null);

        IEnumerable<Identity> GetUnderlyingEntitiesForReview(Identity sectionId);

        IEnumerable<Identity> GetUnderlyingEntitiesForReviewRecursive(Identity sectionId);

        IEnumerable<IInterviewTreeNode> GetAllInterviewNodes();

        IEnumerable<Identity> GetAllIdentitiesForEntityId(Guid id);

        bool AcceptsInterviewerAnswers();

        IReadOnlyCollection<IInterviewTreeNode> GetAllSections();

        InterviewSynchronizationDto GetSynchronizationDto();

        bool IsReadOnlyQuestion(Identity identity);

        InterviewKey GetInterviewKey();

        int? GetAssignmentId();

        bool IsParentOf(Identity parentIdentity, Identity childIdentity);

        bool IsAnswerProtected(Identity questionIdentity, decimal value);


    }
}
