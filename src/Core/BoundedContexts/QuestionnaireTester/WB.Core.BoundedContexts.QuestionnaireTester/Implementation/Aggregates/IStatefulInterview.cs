using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates
{
    public interface IStatefulInterview
    {
        string QuestionnaireId { get; set; }
        long QuestionnaireVersion { get; set; }
        Guid Id { get; set; }
        IReadOnlyDictionary<string, BaseInterviewAnswer> Answers { get; }
        IReadOnlyDictionary<string, List<Identity>> RosterInstancesIds { get; }
        
        bool HasErrors { get; set; }
        bool IsCompleted { get; set; }

        InterviewRoster GetRoster(Identity identity);

        GpsCoordinatesAnswer GetGpsCoordinatesAnswer(Identity identity);

        DateTimeAnswer GetDateTimeAnswer(Identity identity);

        MultimediaAnswer GetMultimediaAnswer(Identity identity);

        QRBarcodeAnswer GetQRBarcodeAnswer(Identity identity);

        TextListAnswer GetTextListAnswer(Identity identity);

        LinkedSingleOptionAnswer GetLinkedSingleOptionAnswer(Identity identity);

        MultiOptionAnswer GetMultiOptionAnswer(Identity identity);

        LinkedMultiOptionAnswer GetLinkedMultiOptionAnswer(Identity identity);

        IntegerNumericAnswer GetIntegerNumericAnswer(Identity identity);

        RealNumericAnswer GetRealNumericAnswer(Identity identity);

        TextAnswer GetTextAnswer(Identity identity);

        SingleOptionAnswer GetSingleOptionAnswer(Identity identity);

        bool IsValid(Identity identity);

        bool IsEnabled(Identity entityIdentity);

        bool WasAnswered(Identity entityIdentity);

        string GetInterviewerAnswerComment(Identity entityIdentity);

        string GetRosterTitle(Identity rosterIdentity);

        /// <summary>
        /// Gets an answer by roster vector that will be reduced until requested question is found.
        /// </summary>
        /// <returns>null if question is not answered yet.</returns>
        BaseInterviewAnswer FindBaseAnswerByOrDeeperRosterLevel(Guid questionId, decimal[] targetRosterVector);

        IEnumerable<BaseInterviewAnswer> FindAnswersOfReferencedQuestionForLinkedQuestion(Guid referencedQuestionId, Identity linkedQuestion);

        InterviewRoster FindRosterByOrDeeperRosterLevel(Guid rosterId, decimal[] targetRosterVector);

        IEnumerable<string> GetParentRosterTitlesWithoutLast(Guid questionId, decimal[] rosterVector);

        int CountInterviewerQuestionsInGroupRecursively(Identity groupIdentity);

        int CountActiveInterviewerQuestionsInGroupOnly(Identity group);

        int GetGroupsInGroupCount(Identity group);

        int CountAnsweredInterviewerQuestionsInGroupRecursively(Identity groupIdentity);

        int CountAnsweredInterviewerQuestionsInGroupOnly(Identity group);

        int CountInvalidInterviewerAnswersInGroupRecursively(Identity groupIdentity);

        int CountInvalidInterviewerQuestionsInGroupOnly(Identity group);

        bool HasInvalidInterviewerQuestionsInGroupOnly(Identity group);

        bool HasUnansweredInterviewerQuestionsInGroupOnly(Identity group);

        IEnumerable<Identity> GetChildQuestions(Identity groupIdentity);

        IEnumerable<Identity> GetEnabledGroupInstances(Guid groupId, decimal[] parentRosterVector);

        IEnumerable<Identity> GetEnabledSubgroups(Identity group);
    }
}