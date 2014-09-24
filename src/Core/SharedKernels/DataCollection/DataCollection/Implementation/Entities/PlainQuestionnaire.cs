using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    internal class PlainQuestionnaire : IQuestionnaire
    {
        #region State

        private readonly QuestionnaireDocument innerDocument = new QuestionnaireDocument();
        private readonly Func<long> getVersion;

        private Dictionary<Guid, IQuestion> questionCache = null;
        private Dictionary<Guid, IGroup> groupCache = null;

        private readonly Dictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingQuestions = new Dictionary<Guid, IEnumerable<Guid>>();
        private readonly Dictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingMandatoryQuestions = new Dictionary<Guid, IEnumerable<Guid>>();
        private readonly Dictionary<Guid, IEnumerable<Guid>> cacheOfRostersAffectedByRosterTitleQuestion = new Dictionary<Guid, IEnumerable<Guid>>();

        internal QuestionnaireDocument QuestionnaireDocument
        {
            get { return this.innerDocument; }
        }

        internal Dictionary<Guid, IQuestion> QuestionCache
        {
            get
            {
                return this.questionCache ?? (this.questionCache
                    = this.innerDocument
                        .Find<IQuestion>(_ => true)
                        .ToDictionary(
                            question => question.PublicKey,
                            question => question));
            }
        }

        internal Dictionary<Guid, IGroup> GroupCache
        {
            get
            {
                return this.groupCache ?? (this.groupCache
                    = this.innerDocument
                        .Find<IGroup>(_ => true)
                        .ToDictionary(
                            group => group.PublicKey,
                            group => group));
            }
        }

        #endregion

        public PlainQuestionnaire(QuestionnaireDocument document, Func<long> getVersion)
        {
            InitializeQuestionnaireDocument(document);

            this.innerDocument = document;
            this.getVersion = getVersion;
        }

        public PlainQuestionnaire(QuestionnaireDocument document, long version)
            : this(document, () => version) { }

        public PlainQuestionnaire(QuestionnaireDocument document, Func<long> getVersion,
            Dictionary<Guid, IGroup> groupCache, Dictionary<Guid, IQuestion> questionCache)
            : this(document, getVersion)
        {
            this.groupCache = groupCache;
            this.questionCache = questionCache;
        }


        public long Version { get { return this.getVersion(); } }

        public void InitializeQuestionnaireDocument()
        {
            InitializeQuestionnaireDocument(this.innerDocument);
        }

        public IQuestion GetQuestionByStataCaption(string stataCaption)
        {
            return GetQuestionByStataCaption(this.QuestionCache, stataCaption);
        }

        public bool HasQuestion(Guid questionId)
        {
            return this.GetQuestion(questionId) != null;
        }

        public bool HasGroup(Guid groupId)
        {
            return this.GetGroup(groupId) != null;
        }

        public QuestionType GetQuestionType(Guid questionId)
        {
            return this.GetQuestionOrThrow(questionId).QuestionType;
        }

        public bool IsQuestionLinked(Guid questionId)
        {
            return this.GetQuestionOrThrow(questionId).LinkedToQuestionId.HasValue;
        }
        
        public string GetQuestionTitle(Guid questionId)
        {
            return this.GetQuestionOrThrow(questionId).QuestionText;
        }

        public string GetQuestionVariableName(Guid questionId)
        {
            return this.GetQuestionOrThrow(questionId).StataExportCaption;
        }

        public string GetGroupTitle(Guid groupId)
        {
            return this.GetGroupOrThrow(groupId).Title;
        }

        public Guid? GetCascadingQuestionParentId(Guid questionId)
        {
            return this.GetQuestionOrThrow(questionId).CascadeFromQuestionId;
        }

        public IEnumerable<decimal> GetAnswerOptionsAsValues(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            bool questionTypeDoesNotSupportAnswerOptions
                = question.QuestionType != QuestionType.SingleOption && question.QuestionType != QuestionType.MultyOption;

            if (questionTypeDoesNotSupportAnswerOptions)
                throw new QuestionnaireException(string.Format(
                    "Cannot return answer options for question with id '{0}' because it's type {1} does not support answer options.",
                    questionId, question.QuestionType));

            return question.Answers.Select(answer => this.ParseAnswerOptionValueOrThrow(answer.AnswerValue, questionId)).ToList();
        }

        public string GetAnswerOptionTitle(Guid questionId, decimal answerOptionValue)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            bool questionTypeDoesNotSupportAnswerOptions
                = question.QuestionType != QuestionType.SingleOption && question.QuestionType != QuestionType.MultyOption;

            if (questionTypeDoesNotSupportAnswerOptions)
                throw new QuestionnaireException(string.Format(
                    "Cannot return answer option title for question with id '{0}' because it's type {1} does not support answer options.",
                    questionId, question.QuestionType));

            return question
                .Answers
                .Single(answer => answerOptionValue == this.ParseAnswerOptionValueOrThrow(answer.AnswerValue, questionId))
                .AnswerText;
        }

        public string GetCascadingParentValue(Guid questionId, decimal answerOptionValue)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            bool questionTypeDoesNotSupportAnswerOptions
                = question.QuestionType != QuestionType.SingleOption && question.QuestionType != QuestionType.MultyOption;

            if (questionTypeDoesNotSupportAnswerOptions)
                throw new QuestionnaireException(string.Format(
                    "Cannot return answer option title for question with id '{0}' because it's type {1} does not support answer options.",
                    questionId, question.QuestionType));

            return question
                .Answers
                .Single(answer => answerOptionValue == this.ParseAnswerOptionValueOrThrow(answer.AnswerValue, questionId))
                .ParentValue;
        }

        public int? GetMaxSelectedAnswerOptions(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            bool questionTypeDoesNotSupportMaxSelectedAnswerOptions = question.QuestionType != QuestionType.MultyOption;

            if (questionTypeDoesNotSupportMaxSelectedAnswerOptions || !(question is IMultyOptionsQuestion))
                throw new QuestionnaireException(string.Format(
                    "Cannot return maximum for selected answers for question with id '{0}' because it's type {1} does not support that parameter.",
                    questionId, question.QuestionType));

            return ((IMultyOptionsQuestion)question).MaxAllowedAnswers;
        }

        public bool IsCustomValidationDefined(Guid questionId)
        {
            var validationExpression = this.GetCustomValidationExpression(questionId);

            return IsExpressionDefined(validationExpression);
        }

        public string GetCustomValidationExpression(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            return question.ValidationExpression;
        }

        public IEnumerable<Guid> GetAllParentGroupsForQuestion(Guid questionId)
        {
            return this.GetAllParentGroupsForQuestionStartingFromBottom(questionId);
        }

        public string GetCustomEnablementConditionForQuestion(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            return question.ConditionExpression;
        }

        public string GetCustomEnablementConditionForGroup(Guid groupId)
        {
            IGroup group = this.GetGroupOrThrow(groupId);

            return group.ConditionExpression;
        }

        public bool ShouldQuestionSpecifyRosterSize(Guid questionId)
        {
            return this.DoesQuestionSupportRoster(questionId)
                && this.GetRosterGroupsByRosterSizeQuestion(questionId).Any();
        }

        public IEnumerable<Guid> GetRosterGroupsByRosterSizeQuestion(Guid questionId)
        {
            if (!this.DoesQuestionSupportRoster(questionId))
                return Enumerable.Empty<Guid>();

            //### old questionnaires supporting
            IQuestion question = this.GetQuestionOrThrow(questionId);
            var autoPropagatingQuestion = question as IAutoPropagateQuestion;
            if (autoPropagatingQuestion != null)
            {
                foreach (Guid groupId in autoPropagatingQuestion.Triggers)
                {
                    this.ThrowIfGroupDoesNotExist(groupId,
                        string.Format("Propagating question with id '{0}' references missing group.",
                            FormatQuestionForException(autoPropagatingQuestion)));
                }

                return autoPropagatingQuestion.Triggers.ToList();
            }

            //### roster
            return this.GetAllGroups().Where(x => x.RosterSizeQuestionId == questionId && x.IsRoster).Select(x => x.PublicKey);
        }

        public int? GetMaxValueForNumericQuestion(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            this.ThrowIfQuestionDoesNotSupportRoster(question.PublicKey);

            //### old questionnaires supporting
            var autoPropagatingQuestion = question as IAutoPropagateQuestion;
            if (autoPropagatingQuestion != null)
                return autoPropagatingQuestion.MaxValue;

            //### roster
            var numericQuestion = question as INumericQuestion;
            if (numericQuestion != null)
                return numericQuestion.MaxValue;

            return null;
        }

        public int? GetListSizeForListQuestion(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            this.ThrowIfQuestionDoesNotSupportRoster(question.PublicKey);

            var listQuestion = question as ITextListQuestion;
            if (listQuestion != null)
                return listQuestion.MaxAnswerCount;

            return null;
        }

        public IEnumerable<Guid> GetRostersFromTopToSpecifiedQuestion(Guid questionId)
        {
            return this
                .GetAllParentGroupsForQuestionStartingFromBottom(questionId)
                .Where(this.IsRosterGroup)
                .Reverse()
                .ToList();
        }

        public IEnumerable<Guid> GetRostersFromTopToSpecifiedGroup(Guid groupId)
        {
            IGroup group = this.GetGroupOrThrow(groupId);

            return this
                .GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(@group)
                .Where(this.IsRosterGroup)
                .Reverse()
                .ToList();
        }

        public IEnumerable<Guid> GetFixedRosterGroups(Guid? parentRosterId = null)
        {
            if (parentRosterId.HasValue)
            {
                var nestedRosters = this.GetNestedRostersOfGroupById(parentRosterId.Value);
                return this
                    .GetAllGroups()
                    .Where(x => nestedRosters.Contains(x.PublicKey) && x.RosterSizeSource == RosterSizeSourceType.FixedTitles)
                    .Select(x => x.PublicKey)
                    .ToList();
            }
            return this
                .GetAllGroups()
                .Where(
                    x =>
                        x.IsRoster && x.RosterSizeSource == RosterSizeSourceType.FixedTitles &&
                            this.GetRosterLevelForGroup(x.PublicKey) == 1)
                .Select(x => x.PublicKey)
                .ToList();
        }

        public int GetRosterLevelForQuestion(Guid questionId)
        {
            this.ThrowIfQuestionDoesNotExist(questionId);

            return this.GetRosterLevelForQuestion(questionId, this.GetAllParentGroupsForQuestion, this.IsRosterGroup);
        }

        public int GetRosterLevelForGroup(Guid groupId)
        {
            IGroup group = this.GetGroupOrThrow(groupId);

            return this
                .GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(group)
                .Count(this.IsRosterGroup);
        }

        public IEnumerable<Guid> GetAllMandatoryQuestions()
        {
            return
                from question in this.GetAllQuestions()
                where question.Mandatory
                select question.PublicKey;
        }

        public bool IsRosterGroup(Guid groupId)
        {
            IGroup @group = this.GetGroupOrThrow(groupId);

            return IsRosterGroup(@group);
        }

        public IEnumerable<Guid> GetAllUnderlyingQuestions(Guid groupId)
        {
            if (!this.cacheOfUnderlyingQuestions.ContainsKey(groupId))
                this.cacheOfUnderlyingQuestions[groupId] = this.GetAllUnderlyingQuestionsImpl(groupId);

            return this.cacheOfUnderlyingQuestions[groupId];
        }

        public Guid GetQuestionReferencedByLinkedQuestion(Guid linkedQuestionId)
        {
            IQuestion linkedQuestion = this.GetQuestionOrThrow(linkedQuestionId);

            if (!linkedQuestion.LinkedToQuestionId.HasValue)
                throw new QuestionnaireException(string.Format(
                    "Cannot return id of referenced question because specified question {0} is not linked.",
                    FormatQuestionForException(linkedQuestion)));

            return linkedQuestion.LinkedToQuestionId.Value;
        }

        public bool IsQuestionMandatory(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            return question.Mandatory;
        }

        public bool IsQuestionInteger(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            //### old questionnaires supporting
            var autoPropagateQuestion = question as IAutoPropagate;
            if (autoPropagateQuestion != null)
                return true;

            //### roster
            var numericQuestion = question as INumericQuestion;
            if (numericQuestion == null)
                throw new QuestionnaireException(string.Format("Question with id '{0}' must be numeric.", questionId));

            return numericQuestion.IsInteger;
        }

        public int? GetCountOfDecimalPlacesAllowedByQuestion(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            var numericQuestion = question as INumericQuestion;
            if (numericQuestion == null)
                throw new QuestionnaireException(string.Format("Question with id '{0}' must be numeric.", questionId));
            if (numericQuestion.IsInteger)
                throw new QuestionnaireException(string.Format("Question with id '{0}' must be real.", questionId));

            return numericQuestion.CountOfDecimalPlaces;
        }

        public IEnumerable<string> GetFixedRosterTitles(Guid groupId)
        {
            var group = this.GetGroup(groupId);
            if (group == null || !group.IsRoster || group.RosterSizeSource != RosterSizeSourceType.FixedTitles)
            {
                return Enumerable.Empty<string>();
            }
            return group.RosterFixedTitles;
        }

        public bool DoesQuestionSpecifyRosterTitle(Guid questionId)
        {
            return this.GetRostersAffectedByRosterTitleQuestion(questionId).Any();
        }

        public IEnumerable<Guid> GetRostersAffectedByRosterTitleQuestion(Guid questionId)
        {
            if (!this.cacheOfRostersAffectedByRosterTitleQuestion.ContainsKey(questionId))
                this.cacheOfRostersAffectedByRosterTitleQuestion[questionId] = this.GetRostersAffectedByRosterTitleQuestionImpl(questionId);

            return this.cacheOfRostersAffectedByRosterTitleQuestion[questionId];
        }

        public IEnumerable<Guid> GetNestedRostersOfGroupById(Guid rosterId)
        {
            var roster = this.GetGroupOrThrow(rosterId);

            var nestedRosters = new List<Guid>();
            var nestedGroups = new Queue<IGroup>(roster.Children.OfType<IGroup>());

            while (nestedGroups.Count > 0)
            {
                var currentGroup = nestedGroups.Dequeue();
                if (IsRosterGroup(currentGroup))
                {
                    nestedRosters.Add(currentGroup.PublicKey);
                    continue;
                }
                foreach (var childGroup in currentGroup.Children.OfType<IGroup>())
                {
                    nestedGroups.Enqueue(childGroup);
                }
            }

            return nestedRosters;
        }

        public Guid? GetRosterSizeQuestion(Guid rosterId)
        {
            var roster = this.GetGroupOrThrow(rosterId);
            return roster.RosterSizeQuestionId;
        }

        public IEnumerable<Guid> GetCascadingQuestionsThatDependUponQuestion(Guid questionId)
        {
            var result = new List<Guid>();
            var foundItems = new List<Guid>{questionId};
            bool itemsAdded = true;

            while (itemsAdded)
            {
                itemsAdded = false;
                foreach (var foundItem in foundItems)
                {
                    foundItems = this.QuestionnaireDocument.Children
                        .TreeToEnumerable(x => x.Children)
                        .Where(x =>
                        {
                            var question = x as SingleQuestion;
                            var isCascadingQuestion = question != null &&
                                question.CascadeFromQuestionId.HasValue;
                            if (isCascadingQuestion)
                            {
                                return question.CascadeFromQuestionId == foundItem;
                            }
                            return false;
                        }).Select(x => x.PublicKey).ToList();

                    itemsAdded = itemsAdded || foundItems.Count > 0;
                    result.AddRange(foundItems);
                }
            }

            return result;
        }

        public IEnumerable<Guid> GetCascadingQuestionsThatDirectlyDependUponQuestion(Guid id)
        {
            return this.QuestionnaireDocument.Children.TreeToEnumerable(_ => _.Children)
                .Where(x =>
                {
                    var question = x as AbstractQuestion;
                    return question != null && question.CascadeFromQuestionId.HasValue && question.CascadeFromQuestionId.Value == id;
                }).Select(x => x.PublicKey);
        }

        public IEnumerable<Guid> GetAllChildCascadingQuestions()
        {
            return this.QuestionnaireDocument.Children.TreeToEnumerable(_ => _.Children)
                .Where(x =>
                {
                    var question = x as AbstractQuestion;
                    return question != null && question.CascadeFromQuestionId.HasValue;
                }).Select(x => x.PublicKey);
        }

        public IEnumerable<Guid> GetUnderlyingMandatoryQuestions(Guid groupId)
        {
            if (!this.cacheOfUnderlyingMandatoryQuestions.ContainsKey(groupId))
                this.cacheOfUnderlyingMandatoryQuestions[groupId] = this.GetUnderlyingMandatoryQuestionsImpl(groupId);

            return this.cacheOfUnderlyingMandatoryQuestions[groupId];
        }

        private IEnumerable<Guid> GetRostersAffectedByRosterTitleQuestionImpl(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            Guid? rosterAffectedByBackwardCompatibility =
                question.Capital
                    ? this.GetRostersFromTopToSpecifiedQuestion(questionId).Cast<Guid?>().LastOrDefault()
                    : null;

            IEnumerable<Guid> rostersAffectedByCurrentDomain =
                from @group in this.GetAllGroups()
                where this.IsRosterGroup(@group.PublicKey) && @group.RosterTitleQuestionId == questionId
                select @group.PublicKey;

            return Enumerable.ToList(
                rosterAffectedByBackwardCompatibility.HasValue
                    ? rostersAffectedByCurrentDomain.Union(new[] { rosterAffectedByBackwardCompatibility.Value })
                    : rostersAffectedByCurrentDomain);
        }

        private IEnumerable<Guid> GetAllUnderlyingQuestionsImpl(Guid groupId)
        {
            return this
                .GetGroupOrThrow(groupId)
                .Find<IQuestion>(_ => true)
                .Select(question => question.PublicKey)
                .ToList();
        }

        private IEnumerable<Guid> GetUnderlyingMandatoryQuestionsImpl(Guid groupId)
        {
            return this
                .GetGroupOrThrow(groupId)
                .Find<IQuestion>(question => question.Mandatory)
                .Select(question => question.PublicKey)
                .ToList();
        }

        private IEnumerable<IGroup> GetAllGroups()
        {
            return this.GroupCache.Values;
        }

        private IEnumerable<IQuestion> GetAllQuestions()
        {
            return this.QuestionCache.Values;
        }

        private int GetRosterLevelForQuestion(Guid questionId, Func<Guid, IEnumerable<Guid>> getAllParentGroupsForQuestion,
            Func<Guid, bool> isRosterGroup)
        {
            return getAllParentGroupsForQuestion(questionId).Count(isRosterGroup);
        }

        private IEnumerable<Guid> GetAllParentGroupsForQuestionStartingFromBottom(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            var parentGroup = (IGroup)question.GetParent();

            return this.GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(parentGroup);
        }

        private IEnumerable<Guid> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(IGroup group)
        {
            return this.GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(group, this.innerDocument).Select(_ => _.PublicKey);
        }

        private IEnumerable<IGroup> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(IGroup group, QuestionnaireDocument document)
        {
            var parentGroups = new List<IGroup>();

            while (group != document)
            {
                parentGroups.Add(group);
                group = (IGroup)group.GetParent();
            }

            return parentGroups;
        }

        private decimal ParseAnswerOptionValueOrThrow(string value, Guid questionId)
        {
            decimal parsedValue;

            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
                throw new QuestionnaireException(string.Format(
                    "Cannot parse answer option value '{0}' as decimal. Question id: '{1}'.",
                    value, questionId));

            return parsedValue;
        }

        private static bool IsExpressionDefined(string expression)
        {
            return !string.IsNullOrWhiteSpace(expression);
        }

        private bool DoesQuestionSupportRoster(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            return DoesQuestionSupportRoster(question);
        }

        private static bool DoesQuestionSupportRoster(IQuestion question)
        {
            //### roster
            return question.QuestionType == QuestionType.Numeric
                || question.QuestionType == QuestionType.MultyOption
                || question.QuestionType == QuestionType.TextList
                //### old questionnaires supporting
                || (question.QuestionType == QuestionType.AutoPropagate && question is IAutoPropagateQuestion);
        }

        private static bool IsRosterGroup(IGroup group)
        {
            //### old questionnaires supporting                    //### roster
            return group.Propagated == Propagate.AutoPropagated || group.IsRoster;
        }

        private void ThrowIfQuestionDoesNotSupportRoster(Guid questionId)
        {
            if (!this.DoesQuestionSupportRoster(questionId))
                throw new QuestionnaireException(string.Format("Question with id '{0}' is not a roster size question.", questionId));
        }

        private void ThrowIfGroupDoesNotExist(Guid groupId, string customExceptionMessage = null)
        {
            this.GetGroupOrThrow(groupId, customExceptionMessage);
        }

        private IGroup GetGroupOrThrow(Guid groupId, string customExceptionMessage = null)
        {
            IGroup group = this.GetGroup(groupId);

            if (group == null)
                throw new QuestionnaireException(customExceptionMessage ?? string.Format("Group with id '{0}' is not found.", groupId));

            return group;
        }

        private IGroup GetGroup(Guid groupId)
        {
            return GetGroup(this.GroupCache, groupId);
        }

        private void ThrowIfQuestionDoesNotExist(Guid questionId)
        {
            this.GetQuestionOrThrow(questionId);
        }

        private IQuestion GetQuestionOrThrow(Guid questionId)
        {
            return GetQuestionOrThrow(this.QuestionCache, questionId);
        }

        private IQuestion GetQuestion(Guid questionId)
        {
            return GetQuestion(this.QuestionCache, questionId);
        }

        private static string FormatQuestionForException(IQuestion question)
        {
            return string.Format("'{0} [{1}] ({2:N})'",
                question.QuestionText ?? "<<NO QUESTION TITLE>>",
                question.StataExportCaption ?? "<<NO VARIABLE NAME>>",
                question.PublicKey);
        }

        private static void InitializeQuestionnaireDocument(QuestionnaireDocument source)
        {
            source.ConnectChildrenWithParent();

            if (source.IsCacheWarmed)
                return;

            var groups = source
                .Find<IGroup>(_ => true)
                .ToDictionary(
                    @group => @group.PublicKey,
                    @group => @group);

            var questions = source
                .Find<IQuestion>(_ => true)
                .ToDictionary(
                    question => question.PublicKey,
                    question => question);

            var questionWarmingUpMethods = new Action<Guid>[]
            {
                questionId =>
                {
                    IQuestion question = GetQuestionOrThrow(questions, questionId);
                    question.QuestionIdsInvolvedInCustomValidationOfQuestion =
                        GetQuestionsInvolvedInExpression(questions, question.PublicKey, question.ValidationExpression).ToList();
                },

                questionId => SetQuestionsInvolvedInCustomEnablementConditionOfQuestion(questions, questionId),
                questionId => SetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(questions, questionId),
                questionId => SetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questions, questionId),
                questionId => SetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questions, groups, questionId)
            };

            foreach (IGroup @group in groups.Values)
            {
                try
                {
                    SetQuestionsInvolvedInCustomEnablementConditionOfGroup(questions, groups, @group.PublicKey);
                }
                catch { }
            }

            foreach (Action<Guid> method in questionWarmingUpMethods)
            {
                foreach (IQuestion question in questions.Values)
                {

                    try
                    {
                        method.Invoke(question.PublicKey);
                    }
                    catch { }
                }
            }

            source.IsCacheWarmed = true;
        }

        #region warmup caches

        private static void SetQuestionsInvolvedInCustomEnablementConditionOfQuestion(Dictionary<Guid, IQuestion> questions, Guid questionId)
        {
            IQuestion question = GetQuestionOrThrow(questions, questionId);
            question.QuestionIdsInvolvedInCustomEnablementConditionOfQuestion =
                GetQuestionsInvolvedInExpression(questions, question.PublicKey, question.ConditionExpression).ToList();
        }

        private static void SetQuestionsInvolvedInCustomEnablementConditionOfGroup(Dictionary<Guid, IQuestion> questions,
            Dictionary<Guid, IGroup> groups, Guid questionId)
        {
            IGroup group = GetGroup(groups, questionId);
            group.QuestionIdsInvolvedInCustomEnablementConditionOfGroup =
                GetQuestionsInvolvedInExpression(questions, group.PublicKey, group.ConditionExpression).ToList();
        }

        private static void SetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(Dictionary<Guid, IQuestion> questions, Guid questionId)
        {
            var targetQuestion = GetQuestion(questions, questionId);
            targetQuestion.QuestionsWhichCustomValidationDependsOnQuestion = Enumerable.ToList(
                from question in questions.Values
                where
                    DoesQuestionCustomValidationDependOnSpecifiedQuestion(questions, question.PublicKey,
                        specifiedQuestionId: questionId)
                        && questionId != question.PublicKey
                select question.PublicKey
                );
        }

        private static void SetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Dictionary<Guid, IQuestion> questions,
            Guid questionId)
        {
            var targetQuestion = GetQuestion(questions, questionId);
            targetQuestion.ConditionalDependentQuestions = Enumerable.ToList(
                from question in questions.Values
                where
                    DoesQuestionCustomEnablementDependOnSpecifiedQuestion(questions, question.PublicKey,
                        specifiedQuestionId: questionId)
                        && questionId != question.PublicKey
                select question.PublicKey
                );
        }

        private static void SetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Dictionary<Guid, IQuestion> questions,
            Dictionary<Guid, IGroup> groups, Guid questionId)
        {
            var targetQuestion = GetQuestion(questions, questionId);
            targetQuestion.ConditionalDependentGroups = Enumerable.ToList(
                from @group in groups.Values
                where DoesGroupCustomEnablementDependOnSpecifiedQuestion(groups, @group.PublicKey, specifiedQuestionId: questionId)
                select @group.PublicKey
                );
        }

        private static IEnumerable<Guid> GetQuestionsInvolvedInExpression(Dictionary<Guid, IQuestion> questions, Guid contextQuestionId,
            string expression)
        {
            return Enumerable.Empty<Guid>();
        }

        private static IQuestion GetQuestionOrThrow(Dictionary<Guid, IQuestion> questions, Guid questionId)
        {
            IQuestion question = GetQuestion(questions, questionId);

            if (question == null)
                throw new QuestionnaireException(string.Format("Question with id '{0}' is not found.", questionId));

            return question;
        }

        private static IQuestion GetQuestion(Dictionary<Guid, IQuestion> questions, Guid questionId)
        {
            return questions.ContainsKey(questionId)
                ? questions[questionId]
                : null;
        }

        private static IGroup GetGroup(Dictionary<Guid, IGroup> groups, Guid groupId)
        {
            return groups.ContainsKey(groupId)
                ? groups[groupId]
                : null;
        }

        private static IQuestion GetQuestionByStataCaption(Dictionary<Guid, IQuestion> questions, string identifier)
        {
            return questions.Values.FirstOrDefault(q => q.StataExportCaption == identifier);
        }

        private static bool DoesQuestionCustomValidationDependOnSpecifiedQuestion(Dictionary<Guid, IQuestion> questions, Guid questionId,
            Guid specifiedQuestionId)
        {
            var question = GetQuestion(questions, questionId);

            IEnumerable<Guid> involvedQuestions = question.QuestionIdsInvolvedInCustomValidationOfQuestion;

            bool isSpecifiedQuestionInvolved = involvedQuestions.Contains(specifiedQuestionId);

            return isSpecifiedQuestionInvolved;
        }

        private static bool DoesQuestionCustomEnablementDependOnSpecifiedQuestion(Dictionary<Guid, IQuestion> questions, Guid questionId,
            Guid specifiedQuestionId)
        {
            var question = GetQuestion(questions, questionId);

            IEnumerable<Guid> involvedQuestions = question.QuestionIdsInvolvedInCustomEnablementConditionOfQuestion;

            bool isSpecifiedQuestionInvolved = involvedQuestions.Contains(specifiedQuestionId);

            return isSpecifiedQuestionInvolved;
        }

        private static bool DoesGroupCustomEnablementDependOnSpecifiedQuestion(Dictionary<Guid, IGroup> groups, Guid groupId,
            Guid specifiedQuestionId)
        {
            var group = GetGroup(groups, groupId);

            IEnumerable<Guid> involvedQuestions = group.QuestionIdsInvolvedInCustomEnablementConditionOfGroup;

            bool isSpecifiedQuestionInvolved = involvedQuestions.Contains(specifiedQuestionId);

            return isSpecifiedQuestionInvolved;
        }

        #endregion

    }
}