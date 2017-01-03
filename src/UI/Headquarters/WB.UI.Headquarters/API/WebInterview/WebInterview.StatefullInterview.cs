using System;
using System.Collections.Generic;
using System.Globalization;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview
    {
        public InterviewStatus Status => this.CurrentInterview.Status;

        public string InterviewerCompleteComment => this.CurrentInterview.InterviewerCompleteComment;
        public string SupervisorRejectComment => this.CurrentInterview.SupervisorRejectComment;

        public string GetAnswerAsString(Identity questionIdentity, CultureInfo cultureInfo = null)
            => this.CurrentInterview.GetAnswerAsString(questionIdentity, cultureInfo);

        public bool HasErrors => this.CurrentInterview.HasErrors;
        public bool IsCompleted => this.CurrentInterview.IsCompleted;
        public bool CreatedOnClient => this.CurrentInterview.CreatedOnClient;

        public InterviewTreeGroup GetGroup(Identity identity) => this.CurrentInterview.GetGroup(identity);
        public InterviewTreeRoster GetRoster(Identity identity) => this.CurrentInterview.GetRoster(identity);

        public InterviewTreeGpsQuestion GetGpsQuestion(Identity identity)
            => this.CurrentInterview.GetGpsQuestion(identity);

        public InterviewTreeDateTimeQuestion GetDateTimeQuestion(Identity identity)
            => this.CurrentInterview.GetDateTimeQuestion(identity);

        public InterviewTreeMultimediaQuestion GetMultimediaQuestion(Identity identity)
            => this.CurrentInterview.GetMultimediaQuestion(identity);

        public InterviewTreeQRBarcodeQuestion GetQRBarcodeQuestion(Identity identity)
            => this.CurrentInterview.GetQRBarcodeQuestion(identity);

        public InterviewTreeTextListQuestion GetTextListQuestion(Identity identity)
            => this.CurrentInterview.GetTextListQuestion(identity);

        public InterviewTreeSingleLinkedToRosterQuestion GetLinkedSingleOptionQuestion(Identity identity)
            => this.CurrentInterview.GetLinkedSingleOptionQuestion(identity);

        public InterviewTreeMultiOptionQuestion GetMultiOptionQuestion(Identity identity)
            => this.CurrentInterview.GetMultiOptionQuestion(identity);

        public InterviewTreeMultiLinkedToRosterQuestion GetLinkedMultiOptionQuestion(Identity identity)
            => this.CurrentInterview.GetLinkedMultiOptionQuestion(identity);

        public InterviewTreeIntegerQuestion GetIntegerQuestion(Identity identity)
            => this.CurrentInterview.GetIntegerQuestion(identity);

        public InterviewTreeDoubleQuestion GetDoubleQuestion(Identity identity)
            => this.CurrentInterview.GetDoubleQuestion(identity);

        public InterviewTreeTextQuestion GetTextQuestion(Identity identity)
            => this.CurrentInterview.GetTextQuestion(identity);

        public InterviewTreeSingleOptionQuestion GetSingleOptionQuestion(Identity identity)
            => this.CurrentInterview.GetSingleOptionQuestion(identity);

        public InterviewTreeYesNoQuestion GetYesNoQuestion(Identity identity)
            => this.CurrentInterview.GetYesNoQuestion(identity);

        public InterviewTreeMultiOptionLinkedToListQuestion GetMultiOptionLinkedToListQuestion(Identity identity)
            => this.CurrentInterview.GetMultiOptionLinkedToListQuestion(identity);

        public InterviewTreeSingleOptionLinkedToListQuestion GetSingleOptionLinkedToListQuestion(Identity identity)
            => this.CurrentInterview.GetSingleOptionLinkedToListQuestion(identity);

        public bool HasGroup(Identity group) => this.CurrentInterview.HasGroup(group);

        public bool IsEntityValid(Identity identity) => this.CurrentInterview.IsEntityValid(identity);

        public IEnumerable<string> GetFailedValidationMessages(Identity questionOrStaticTextId)
            => this.CurrentInterview.GetFailedValidationMessages(questionOrStaticTextId);

        public bool IsEnabled(Identity entityIdentity) => this.CurrentInterview.IsEnabled(entityIdentity);

        public bool WasAnswered(Identity entityIdentity) => this.CurrentInterview.WasAnswered(entityIdentity);

        public IEnumerable<AnswerComment> GetQuestionComments(Identity entityIdentity)
            => this.CurrentInterview.GetQuestionComments(entityIdentity);

        public string GetRosterTitle(Identity rosterIdentity) => this.CurrentInterview.GetRosterTitle(rosterIdentity);

        public string GetTitleText(Identity entityIdentity) => this.CurrentInterview.GetTitleText(entityIdentity);

        public IEnumerable<string> GetParentRosterTitlesWithoutLast(Identity questionIdentity)
            => this.CurrentInterview.GetParentRosterTitlesWithoutLast(questionIdentity);

        public IEnumerable<string> GetParentRosterTitlesWithoutLastForRoster(Identity rosterIdentity)
            => this.CurrentInterview.GetParentRosterTitlesWithoutLastForRoster(rosterIdentity);

        public int GetGroupsInGroupCount(Identity group) => this.CurrentInterview.GetGroupsInGroupCount(group);

        public int CountEnabledAnsweredQuestions(Identity group)
            => this.CurrentInterview.CountEnabledAnsweredQuestions(group);

        public int CountEnabledInvalidQuestionsAndStaticTexts(Identity group)
            => this.CurrentInterview.CountEnabledInvalidQuestionsAndStaticTexts(group);

        public bool HasEnabledInvalidQuestionsAndStaticTexts(Identity group)
            => this.CurrentInterview.HasEnabledInvalidQuestionsAndStaticTexts(group);

        public bool HasUnansweredQuestions(Identity group) => this.CurrentInterview.HasUnansweredQuestions(group);

        public Identity GetParentGroup(Identity groupOrQuestion)
            => this.CurrentInterview.GetParentGroup(groupOrQuestion);

        public IEnumerable<Identity> GetChildQuestions(Identity groupIdentity)
            => this.CurrentInterview.GetChildQuestions(groupIdentity);

        public IEnumerable<Identity> GetEnabledSubgroups(Identity group)
            => this.CurrentInterview.GetEnabledSubgroups(group);

        public int CountActiveAnsweredQuestionsInInterview
            => this.CurrentInterview.CountActiveAnsweredQuestionsInInterview();

        public int CountActiveQuestionsInInterview => this.CurrentInterview.CountActiveQuestionsInInterview();

        public int CountInvalidEntitiesInInterview => this.CurrentInterview.CountInvalidEntitiesInInterview();

        public object GetVariableValueByOrDeeperRosterLevel(Guid variableId, RosterVector variableRosterVector)
            => this.CurrentInterview.GetVariableValueByOrDeeperRosterLevel(variableId, variableRosterVector);

        public IEnumerable<Identity> GetInvalidEntitiesInInterview()
            => this.CurrentInterview.GetInvalidEntitiesInInterview();

        public bool IsFirstEntityBeforeSecond(Identity first, Identity second)
            => this.CurrentInterview.IsFirstEntityBeforeSecond(first, second);

        public List<CategoricalOption> GetTopFilteredOptionsForQuestion(Identity question, int? parentQuestionValue, string filter, int sliceSize)
            => this.CurrentInterview.GetTopFilteredOptionsForQuestion(question, parentQuestionValue, filter, sliceSize);

        public CategoricalOption GetOptionForQuestionWithoutFilter(Identity question, int value, int? parentQuestionValue = null)
            => this.CurrentInterview.GetOptionForQuestionWithoutFilter(question, value, parentQuestionValue);

        public CategoricalOption GetOptionForQuestionWithFilter(Identity question, string value, int? parentQuestionValue = null)
            => this.CurrentInterview.GetOptionForQuestionWithFilter(question, value, parentQuestionValue);

        public int CountCommentedQuestions => this.CurrentInterview.CountCommentedQuestions();

        public IEnumerable<Identity> GetCommentedBySupervisorQuestionsInInterview()
            => this.CurrentInterview.GetCommentedBySupervisorQuestionsInInterview();

        public string GetLastSupervisorComment() => this.CurrentInterview.GetLastSupervisorComment();

        public IReadOnlyList<Identity> GetRosterInstances(Identity parentIdentity, Guid rosterId)
            => this.CurrentInterview.GetRosterInstances(parentIdentity, rosterId);

        public InterviewTreeQuestion FindQuestionInQuestionBranch(Guid entityId, Identity questionIdentity)
            => this.CurrentInterview.FindQuestionInQuestionBranch(entityId, questionIdentity);

        public bool IsQuestionPrefilled(Identity entityIdentity)
            => this.CurrentInterview.IsQuestionPrefilled(entityIdentity);

        public string GetLinkedOptionTitle(Identity linkedQuestionIdentity, RosterVector option)
            => this.CurrentInterview.GetLinkedOptionTitle(linkedQuestionIdentity, option);

    }
}