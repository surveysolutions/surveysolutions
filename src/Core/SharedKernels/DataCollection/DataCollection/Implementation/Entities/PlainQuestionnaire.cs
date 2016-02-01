using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    internal class PlainQuestionnaire : IQuestionnaire
    {
        public ISubstitutionService SubstitutionService
        {
            get
            {
                return this.substitutionService ??
                       (this.substitutionService = ServiceLocator.Current.GetInstance<ISubstitutionService>());
            }
        }

        private ISubstitutionService substitutionService;

        #region State

        private readonly QuestionnaireDocument innerDocument = new QuestionnaireDocument();
        private readonly Func<long> getVersion;
        private readonly Guid? responsibleId;

        private Dictionary<Guid, IQuestion> questionCache = null;
        private Dictionary<string, IGroup> groupCache = null;
        private Dictionary<Guid, IComposite> entityCache = null;
        private List<Guid> sectionCache = null;

        private Dictionary<string, HashSet<Guid>> cacheOfSubstitutionReferencedQuestions = null;

        private readonly Dictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingGroupsAndRosters = new Dictionary<Guid, IEnumerable<Guid>>();
        private readonly Dictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingGroups = new Dictionary<Guid, IEnumerable<Guid>>();
        private readonly Dictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingRosters = new Dictionary<Guid, IEnumerable<Guid>>();
        private readonly Dictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingQuestions = new Dictionary<Guid, IEnumerable<Guid>>();
        private readonly Dictionary<Guid, IEnumerable<Guid>> cacheOfRostersAffectedByRosterTitleQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
        private readonly Dictionary<Guid, ReadOnlyCollection<Guid>> cacheOfChildQuestions = new Dictionary<Guid, ReadOnlyCollection<Guid>>();
        private readonly Dictionary<Guid, ReadOnlyCollection<Guid>> cacheOfChildEntities = new Dictionary<Guid, ReadOnlyCollection<Guid>>();
        private readonly Dictionary<Guid, ReadOnlyCollection<Guid>> cacheOfChildInterviewerQuestions = new Dictionary<Guid, ReadOnlyCollection<Guid>>();
        private readonly Dictionary<Guid, ReadOnlyCollection<Guid>> cacheOfUnderlyingInterviewerQuestions = new Dictionary<Guid, ReadOnlyCollection<Guid>>();
        private readonly Dictionary<Guid, ReadOnlyCollection<Guid>> cacheOfParentsStartingFromTop = new Dictionary<Guid, ReadOnlyCollection<Guid>>();


        internal QuestionnaireDocument QuestionnaireDocument => this.innerDocument;

        public Guid? ResponsibleId => this.responsibleId;

        private Dictionary<Guid, IComposite> EntityCache
        {
            get
            {
                return this.entityCache ?? (this.entityCache
                    = this.innerDocument
                        .Find<IComposite>(_ => true)
                        .ToDictionary(
                            entity => entity.PublicKey,
                            entity => entity));
            }
        }

        private List<Guid> SectionCache
        {
            get
            {
                return this.sectionCache ?? (this.sectionCache = this.innerDocument.Children.Cast<IGroup>().Where(x => x != null).Select(x => x.PublicKey).ToList());
            }
        }

        private Dictionary<Guid, IQuestion> QuestionCache
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

        private Dictionary<string, IGroup> GroupCache
        {
            get
            {
                return this.groupCache ?? (
                    this.groupCache = this.innerDocument.Find<IGroup>(_ => true)
                        .ToDictionary(
                            group => group.PublicKey.FormatGuid(),
                            group => group));
            }
        }

        private Dictionary<string, HashSet<Guid>> CacheOfSubstitutionReferencedQuestions
        {
            get
            {
                return this.cacheOfSubstitutionReferencedQuestions ??
                       (this.cacheOfSubstitutionReferencedQuestions = this.GetAllSubstitutionReferences());
            }
        }

        #endregion

        public PlainQuestionnaire(QuestionnaireDocument document, Func<long> getVersion, Guid? responsibleId)
        {
            InitializeQuestionnaireDocument(document);

            this.innerDocument = document;
            this.getVersion = getVersion;
            this.responsibleId = responsibleId;
        }

        public PlainQuestionnaire(QuestionnaireDocument document, long version)
            : this(document, () => version, null) {}

        public PlainQuestionnaire(QuestionnaireDocument document, long version, Guid? responsibleId)
            : this(document, () => version, responsibleId) {}

        public PlainQuestionnaire(QuestionnaireDocument document, Func<long> getVersion,
            Dictionary<Guid, IGroup> groupCache, Dictionary<Guid, IQuestion> questionCache, Guid? responsibleId)
            : this(document, getVersion, responsibleId)
        {
            this.groupCache = groupCache.ToDictionary(x => x.Key.FormatGuid(), x => x.Value);
            this.questionCache = questionCache;
        }

        public long Version => this.getVersion();

        public string Title => this.innerDocument.Title;

        public void InitializeQuestionnaireDocument()
        {
            InitializeQuestionnaireDocument(this.innerDocument);
        }

        public IQuestion GetQuestionByStataCaption(string stataCaption) => GetQuestionByStataCaption(this.QuestionCache, stataCaption);

        public bool HasQuestion(Guid questionId) => this.GetQuestion(questionId) != null;

        public bool HasGroup(Guid groupId) => this.GetGroup(groupId) != null;

        public QuestionType GetQuestionType(Guid questionId) => this.GetQuestionOrThrow(questionId).QuestionType;

        public QuestionScope GetQuestionScope(Guid questionId) => this.GetQuestionOrThrow(questionId).QuestionScope;

        public AnswerType GetAnswerType(Guid questionId)
        {
            var questionType = GetQuestionType(questionId);
            switch (questionType)
            {
                case QuestionType.SingleOption:
                    return IsQuestionLinked(questionId)
                        ? AnswerType.RosterVector
                        : AnswerType.OptionCode;

                case QuestionType.MultyOption:
                    return
                        IsQuestionYesNo(questionId)
                            ? AnswerType.YesNoArray
                            : IsQuestionLinked(questionId)
                                ? AnswerType.RosterVectorArray
                                : AnswerType.OptionCodeArray;

                case QuestionType.Numeric:
                    return IsQuestionInteger(questionId)
                        ? AnswerType.Integer
                        : AnswerType.Decimal;

                case QuestionType.DateTime:
                    return AnswerType.DateTime;

                case QuestionType.GpsCoordinates:
                    return AnswerType.GpsData;

                case QuestionType.Text:
                    return AnswerType.String;

                case QuestionType.TextList:
                    return AnswerType.DecimalAndStringArray;

                case QuestionType.QRBarcode:
                    return AnswerType.String;

                case QuestionType.Multimedia:
                    return AnswerType.FileName;
            }

            throw new ArgumentException($"Question of unknown type was found. Question id: {questionId}");
        }

        public bool IsQuestionLinked(Guid questionId) => this.GetQuestionOrThrow(questionId).LinkedToQuestionId.HasValue;

        public string GetQuestionTitle(Guid questionId) => this.GetQuestionOrThrow(questionId).QuestionText;

        public string GetQuestionVariableName(Guid questionId) => this.GetQuestionOrThrow(questionId).StataExportCaption;

        public string GetGroupTitle(Guid groupId) => this.GetGroupOrThrow(groupId).Title;

        public string GetStaticText(Guid staticTextId) => this.GetStaticTextOrThrow(staticTextId).Text;

        public Guid? GetCascadingQuestionParentId(Guid questionId) => this.GetQuestionOrThrow(questionId).CascadeFromQuestionId;

        public IEnumerable<decimal> GetAnswerOptionsAsValues(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            bool questionTypeDoesNotSupportAnswerOptions
                = question.QuestionType != QuestionType.SingleOption && question.QuestionType != QuestionType.MultyOption;

            if (questionTypeDoesNotSupportAnswerOptions)
                throw new QuestionnaireException(
                    $"Cannot return answer options for question with id '{questionId}' because it's type {question.QuestionType} does not support answer options.");

            return question.Answers.Select(answer => ParseAnswerOptionValueOrThrow(answer.AnswerValue, questionId)).ToList();
        }

        public string GetAnswerOptionTitle(Guid questionId, decimal answerOptionValue)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            bool questionTypeDoesNotSupportAnswerOptions
                = question.QuestionType != QuestionType.SingleOption && question.QuestionType != QuestionType.MultyOption;

            if (questionTypeDoesNotSupportAnswerOptions)
                throw new QuestionnaireException(
                    $"Cannot return answer option title for question with id '{questionId}' because it's type {question.QuestionType} does not support answer options.");

            return question
                .Answers
                .Single(answer => answerOptionValue == ParseAnswerOptionValueOrThrow(answer.AnswerValue, questionId))
                .AnswerText;
        }

        public decimal GetCascadingParentValue(Guid questionId, decimal answerOptionValue)
        {
            var question = this.GetQuestionOrThrow(questionId);

            var questionTypeDoesNotSupportAnswerOptions
                = question.QuestionType != QuestionType.SingleOption && question.QuestionType != QuestionType.MultyOption;

            if (questionTypeDoesNotSupportAnswerOptions)
                throw new QuestionnaireException(
                    $"Cannot return answer option title for question with id '{questionId}' because it's type {question.QuestionType} does not support answer options.");

            var stringParentAnswer = question
                .Answers
                .Single(answer => answerOptionValue == ParseAnswerOptionValueOrThrow(answer.AnswerValue, questionId))
                .ParentValue;

            decimal parsedValue;

            if (!decimal.TryParse(stringParentAnswer, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
                throw new QuestionnaireException(
                    $"Cannot parse parent answer option value '{stringParentAnswer}' as decimal. Question id: '{questionId}'.");

            return parsedValue;
        }

        public int? GetMaxSelectedAnswerOptions(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            bool questionTypeDoesNotSupportMaxSelectedAnswerOptions = question.QuestionType != QuestionType.MultyOption;

            if (questionTypeDoesNotSupportMaxSelectedAnswerOptions || !(question is IMultyOptionsQuestion))
                throw new QuestionnaireException(
                    $"Cannot return maximum for selected answers for question with id '{questionId}' because it's type {question.QuestionType} does not support that parameter.");

            return ((IMultyOptionsQuestion)question).MaxAllowedAnswers;
        }

        public int GetMaxRosterRowCount() => Constants.MaxRosterRowCount;

        public bool IsCustomValidationDefined(Guid questionId)
        {
            var validationExpression = this.GetCustomValidationExpression(questionId);

            return IsExpressionDefined(validationExpression);
        }

        public bool IsQuestion(Guid entityId) => this.HasQuestion(entityId);

        public bool IsInterviewierQuestion(Guid questionId)
        {
            var question = this.GetQuestion(questionId);

            return question != null && question.QuestionScope == QuestionScope.Interviewer && !question.Featured;
        }

        public string GetCustomValidationExpression(Guid questionId) => this.GetQuestionOrThrow(questionId).ValidationExpression;

        public ReadOnlyCollection<Guid> GetPrefilledQuestions()
            => this
                .QuestionnaireDocument
                .Find<IQuestion>(question => question.Featured)
                .Select(question => question.PublicKey)
                .ToReadOnlyCollection();

        public IEnumerable<Guid> GetAllParentGroupsForQuestion(Guid questionId) => this.GetAllParentGroupsForQuestionStartingFromBottom(questionId);

        public ReadOnlyCollection<Guid> GetParentsStartingFromTop(Guid entityId)
            => this.cacheOfParentsStartingFromTop.GetOrUpdate(entityId, ()
                => this
                    .GetAllParentGroupsForEntityStartingFromBottom(entityId)
                    .Reverse()
                    .ToReadOnlyCollection());

        public Guid? GetParentGroup(Guid groupOrQuestionId)
        {
            var groupOrQuestion = this.GetGroupOrQuestionOrThrow(groupOrQuestionId);

            IComposite parent = groupOrQuestion.GetParent();

            if (parent == this.innerDocument)
                return null;

            return parent.PublicKey;
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

            return this.GetAllGroups().Where(x => x.RosterSizeQuestionId == questionId && x.IsRoster).Select(x => x.PublicKey);
        }

        public int? GetListSizeForListQuestion(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            this.ThrowIfQuestionDoesNotSupportRoster(question.PublicKey);

            return (question as ITextListQuestion)?.MaxAnswerCount;
        }

        public IEnumerable<Guid> GetRostersFromTopToSpecifiedQuestion(Guid questionId)
        {
            return this
                .GetAllParentGroupsForQuestionStartingFromBottom(questionId)
                .Where(this.IsRosterGroup)
                .Reverse()
                .ToList();
        }

        public IEnumerable<Guid> GetRostersFromTopToSpecifiedEntity(Guid entityId)
        {
            return this
                .GetAllParentGroupsForEntityStartingFromBottom(entityId)
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
                return this.GetAllGroups()
                    .Where(x => nestedRosters.Contains(x.PublicKey) && x.RosterSizeSource == RosterSizeSourceType.FixedTitles)
                    .Select(x => x.PublicKey)
                    .ToList();
            }

            return this
                .GetAllGroups()
                .Where(x
                    => x.IsRoster
                    && x.RosterSizeSource == RosterSizeSourceType.FixedTitles
                    && this.GetRosterLevelForGroup(x.PublicKey) == 1)
                .Select(x => x.PublicKey)
                .ToList();
        }

        public Guid[] GetRosterSizeSourcesForQuestion(Guid questionId)
        {
            this.ThrowIfQuestionDoesNotExist(questionId);

            return this
                .GetRostersFromTopToSpecifiedQuestion(questionId)
                .Select(this.GetRosterSource)
                .ToArray();
        }

        public int GetRosterLevelForQuestion(Guid questionId)
        {
            this.ThrowIfQuestionDoesNotExist(questionId);

            return this.GetRosterLevelForQuestion(questionId, this.GetAllParentGroupsForQuestion, this.IsRosterGroup);
        }

        public int GetRosterLevelForEntity(Guid entityId)
        {
            this.ThrowIfEntityDoesNotExist(entityId);

            return this.GetRosterLevelForEntity(entityId, this.GetAllParentGroupsForEntityStartingFromBottom, this.IsRosterGroup);
        }

        public int GetRosterLevelForGroup(Guid groupId)
        {
            IGroup group = this.GetGroupOrThrow(groupId);

            return this
                .GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(group)
                .Count(this.IsRosterGroup);
        }


        public bool IsRosterGroup(Guid groupId)
        {
            IGroup @group = this.GetGroup(groupId);

            if (@group == null) return false;

            return IsRosterGroup(@group);
        }

        public IEnumerable<Guid> GetAllUnderlyingQuestions(Guid groupId)
            => this.cacheOfUnderlyingQuestions.GetOrUpdate(groupId, this.GetAllUnderlyingQuestionsImpl);

        public ReadOnlyCollection<Guid> GetAllUnderlyingInterviewerQuestions(Guid groupId)
        {
            if (!this.cacheOfUnderlyingInterviewerQuestions.ContainsKey(groupId))
                this.cacheOfUnderlyingInterviewerQuestions[groupId] = this
                    .GetGroupOrThrow(groupId)
                    .Find<IQuestion>(question => question.QuestionScope == QuestionScope.Interviewer && !question.Featured)
                    .Select(question => question.PublicKey)
                    .ToReadOnlyCollection();

            return this.cacheOfUnderlyingInterviewerQuestions[groupId];
        }

        public ReadOnlyCollection<Guid> GetChildQuestions(Guid groupId)
        {
            if (!this.cacheOfChildQuestions.ContainsKey(groupId))
            {
                this.cacheOfChildQuestions[groupId] = new ReadOnlyCollection<Guid>(
                    this.GetGroupOrThrow(groupId)
                        .Children.OfType<IQuestion>()
                        .Select(question => question.PublicKey)
                        .ToList());
            }

            return this.cacheOfChildQuestions[groupId];
        }

        public ReadOnlyCollection<Guid> GetChildEntityIds(Guid groupId)
        {
            if (!this.cacheOfChildEntities.ContainsKey(groupId))
            {
                this.cacheOfChildEntities[groupId] =
                    this.GetGroupOrThrow(groupId)
                        .Children
                        .Select(entity => entity.PublicKey)
                        .ToReadOnlyCollection();
            }

            return this.cacheOfChildEntities[groupId];
        }

        public ReadOnlyCollection<Guid> GetChildInterviewerQuestions(Guid groupId)
            => this.cacheOfChildInterviewerQuestions.GetOrUpdate(groupId, ()
                => this
                    .GetGroupOrThrow(groupId)
                    .Children.OfType<IQuestion>()
                    .Where(question => !question.Featured && question.QuestionScope == QuestionScope.Interviewer)
                    .Select(question => question.PublicKey)
                    .ToReadOnlyCollection());

        public bool IsPrefilled(Guid questionId)
        {
            var question = this.QuestionnaireDocument.Find<IQuestion>(questionId);
            return question.Featured;
        }

        public IEnumerable<Guid> GetAllUnderlyingChildGroupsAndRosters(Guid groupId)
        {
            if (!this.cacheOfUnderlyingGroupsAndRosters.ContainsKey(groupId))
                this.cacheOfUnderlyingGroupsAndRosters[groupId] = this.GetAllUnderlyingGroupsAndRostersImpl(groupId);

            return this.cacheOfUnderlyingGroupsAndRosters[groupId];
        }

        public IEnumerable<Guid> GetAllUnderlyingChildGroups(Guid groupId)
        {
            if (!this.cacheOfUnderlyingGroups.ContainsKey(groupId))
                this.cacheOfUnderlyingGroups[groupId] = this.GetAllUnderlyingGroupsImpl(groupId);

            return this.cacheOfUnderlyingGroups[groupId];
        }

        public IEnumerable<Guid> GetAllUnderlyingChildRosters(Guid groupId)
        {
            if (!this.cacheOfUnderlyingRosters.ContainsKey(groupId))
                this.cacheOfUnderlyingRosters[groupId] = this.GetAllUnderlyingRostersImpl(groupId);

            return this.cacheOfUnderlyingRosters[groupId];
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

        public bool IsQuestionInteger(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            var numericQuestion = question as INumericQuestion;
            if (numericQuestion == null)
                throw new QuestionnaireException(string.Format("Question with id '{0}' must be numeric.", questionId));

            return numericQuestion.IsInteger;
        }

        public bool IsQuestionYesNo(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            var multipleOptionsQuestion = question as IMultyOptionsQuestion;
            if (multipleOptionsQuestion == null)
                throw new QuestionnaireException($"Question with id '{questionId}' must be multiple options question.");

            return multipleOptionsQuestion.YesNoView;
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

        public FixedRosterTitle[] GetFixedRosterTitles(Guid groupId)
        {
            var group = this.GetGroup(groupId);
            if (group == null || !group.IsRoster || group.RosterSizeSource != RosterSizeSourceType.FixedTitles)
            {
                return new FixedRosterTitle[0];
            }
            return group.FixedRosterTitles;
        }

        public bool DoesQuestionSpecifyRosterTitle(Guid questionId) => this.GetRostersAffectedByRosterTitleQuestion(questionId).Any();

        public IEnumerable<Guid> GetRostersAffectedByRosterTitleQuestion(Guid questionId)
        {
            if (!this.cacheOfRostersAffectedByRosterTitleQuestion.ContainsKey(questionId))
            {
                IEnumerable<Guid> rosters = this.GetRostersAffectedByRosterTitleQuestionImpl(questionId);
                this.cacheOfRostersAffectedByRosterTitleQuestion.Add(questionId, rosters);
            }

            return this.cacheOfRostersAffectedByRosterTitleQuestion[questionId];
        }

        public bool IsRosterTitleQuestionAvailable(Guid rosterId)
        {
            IGroup @group = this.GetGroupOrThrow(rosterId);

            return @group.RosterTitleQuestionId.HasValue;
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
            var foundItems = new List<Guid> { questionId };
            bool itemsAdded = true;

            while (itemsAdded)
            {
                itemsAdded = false;
                foreach (var foundItem in foundItems)
                {
                    foundItems = QuestionCache.Values.Where(x =>
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
            return questionCache.Values.Where(x =>
            {
                var question = x as AbstractQuestion;
                return question != null && question.CascadeFromQuestionId.HasValue && question.CascadeFromQuestionId.Value == id;
            }).Select(x => x.PublicKey);
        }

        public IEnumerable<Guid> GetAllChildCascadingQuestions()
        {
            return questionCache.Values.Where(x =>
            {
                var question = x as AbstractQuestion;
                return question != null && question.CascadeFromQuestionId.HasValue;
            }).Select(x => x.PublicKey);
        }

        public bool DoesCascadingQuestionHaveOptionsForParentValue(Guid questionId, decimal parentValue)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            bool questionTypeDoesNotSupportAnswerOptions
                = question.QuestionType != QuestionType.SingleOption && question.QuestionType != QuestionType.MultyOption;

            if (questionTypeDoesNotSupportAnswerOptions)
                throw new QuestionnaireException(string.Format(
                    "Cannot check does question with id '{0}' have options for parent value because it's type {1} does not support answer options.",
                    questionId, question.QuestionType));

            List<decimal> parentValuesFromQuestion = question
                .Answers
                .Select(answer => answer.ParentValue)
                .Where(answerParentValue => answerParentValue != null)
                .Select(answerParentValue => ParseAnswerOptionParentValueOrThrow(answerParentValue, questionId))
                .Distinct()
                .ToList();

            return parentValuesFromQuestion.Contains(parentValue);
        }

        private Dictionary<string, HashSet<Guid>> GetAllSubstitutionReferences()
        {
            var referenceOccurences = new Dictionary<string, HashSet<Guid>>();
            foreach (var question in this.GetAllQuestions())
            {
                var substitutedVariableNames = this.SubstitutionService.GetAllSubstitutionVariableNames(question.QuestionText);

                foreach (var substitutedVariableName in substitutedVariableNames)
                {
                    if (!referenceOccurences.ContainsKey(substitutedVariableName))
                        referenceOccurences.Add(substitutedVariableName, new HashSet<Guid>());
                    if (!referenceOccurences[substitutedVariableName].Contains(question.PublicKey))
                        referenceOccurences[substitutedVariableName].Add(question.PublicKey);
                }
            }
            return referenceOccurences;
        }

        public IEnumerable<Guid> GetSubstitutedQuestions(Guid questionId)
        {
            string targetVariableName = this.GetQuestionVariableName(questionId);

            if (!string.IsNullOrWhiteSpace(targetVariableName))
            {
                if (this.CacheOfSubstitutionReferencedQuestions.ContainsKey(targetVariableName))
                {
                    foreach (var guid in this.CacheOfSubstitutionReferencedQuestions[targetVariableName])
                    {
                        yield return guid;
                    }
                }
            }

            var rostersAffectedByRosterTitleQuestion = this.GetRostersAffectedByRosterTitleQuestion(questionId);
            foreach (var rosterId in rostersAffectedByRosterTitleQuestion)
            {
                IEnumerable<Guid> questionsInRoster = this.GetAllUnderlyingQuestions(rosterId);
                foreach (var questionsInRosterId in questionsInRoster)
                {
                    var questionTitle = GetQuestionTitle(questionsInRosterId);
                    if (this.SubstitutionService.ContainsRosterTitle(questionTitle))
                    {
                        yield return questionsInRosterId;
                    }
                }
            }
        }

        public IEnumerable<Guid> GetAllSections() => this.SectionCache;

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
            => this
                .GetGroupOrThrow(groupId)
                .Find<IQuestion>(_ => true)
                .Select(question => question.PublicKey)
                .ToList();

        private IEnumerable<Guid> GetAllUnderlyingGroupsAndRostersImpl(Guid groupId)
            => this
                .GetGroupOrThrow(groupId)
                .Children
                .Where(child => child is Group).Cast<Group>()
                .Select(group => group.PublicKey)
                .ToList();

        private IEnumerable<Guid> GetAllUnderlyingGroupsImpl(Guid groupId)
            => this
                .GetGroupOrThrow(groupId)
                .Children
                .Where(child => child is Group).Cast<Group>()
                .Where(group => !group.IsRoster)
                .Select(group => group.PublicKey)
                .ToList();

        private IEnumerable<Guid> GetAllUnderlyingRostersImpl(Guid groupId)
        {
            return this
                .GetGroupOrThrow(groupId)
                .Children
                .Where(child => child is Group).Cast<Group>()
                .Where(group => group.IsRoster)
                .Select(group => group.PublicKey)
                .ToList();
        }

        private IEnumerable<IGroup> GetAllGroups() => this.GroupCache.Values;

        private IEnumerable<IQuestion> GetAllQuestions() => this.QuestionCache.Values;

        private int GetRosterLevelForQuestion(Guid questionId, Func<Guid, IEnumerable<Guid>> getAllParentGroupsForQuestion,
            Func<Guid, bool> isRosterGroup)
        {
            return getAllParentGroupsForQuestion(questionId).Count(isRosterGroup);
        }

        private int GetRosterLevelForEntity(Guid entityId, Func<Guid, IEnumerable<Guid>> getAllParentGroupsForEntity,
            Func<Guid, bool> isRosterGroup)
        {
            return getAllParentGroupsForEntity(entityId).Count(isRosterGroup);
        }

        private IEnumerable<Guid> GetAllParentGroupsForQuestionStartingFromBottom(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            var parentGroup = (IGroup)question.GetParent();

            return this.GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(parentGroup);
        }

        private IEnumerable<Guid> GetAllParentGroupsForEntityStartingFromBottom(Guid entityId)
        {
            IComposite entity = this.GetEntityOrThrow(entityId);

            var parentGroup = (IGroup)entity.GetParent();

            return this.GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(parentGroup);
        }

        private IEnumerable<Guid> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(IGroup group)
        {
            return this.GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(group, this.innerDocument).Select(_ => _.PublicKey);
        }

        private IEnumerable<IGroup> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(IGroup group, QuestionnaireDocument document)
        {
            while (group != document)
            {
                yield return group;
                group = (IGroup)group.GetParent();
            }
        }

        private static decimal ParseAnswerOptionValueOrThrow(string value, Guid questionId)
        {
            decimal parsedValue;

            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
                throw new QuestionnaireException(string.Format(
                    "Cannot parse answer option value '{0}' as decimal. Question id: '{1}'.",
                    value, questionId));

            return parsedValue;
        }

        private static decimal ParseAnswerOptionParentValueOrThrow(string value, Guid questionId)
        {
            decimal parsedValue;

            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
                throw new QuestionnaireException(string.Format(
                    "Cannot parse answer option parent value '{0}' as decimal. Question id: '{1}'.",
                    value, questionId));

            return parsedValue;
        }

        private static bool IsExpressionDefined(string expression) => !string.IsNullOrWhiteSpace(expression);

        private bool DoesQuestionSupportRoster(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            return DoesQuestionSupportRoster(question);
        }

        private static bool DoesQuestionSupportRoster(IQuestion question)
        {
            return question.QuestionType == QuestionType.Numeric
                || question.QuestionType == QuestionType.MultyOption
                || question.QuestionType == QuestionType.TextList;
        }

        private static bool IsRosterGroup(IGroup group) => @group.IsRoster;

        private Guid GetRosterSource(Guid rosterId)
        {
            IGroup roster = this.GetGroupOrThrow(rosterId);

            return roster.RosterSizeQuestionId ?? roster.PublicKey;
        }

        private void ThrowIfQuestionDoesNotSupportRoster(Guid questionId)
        {
            if (!this.DoesQuestionSupportRoster(questionId))
                throw new QuestionnaireException($"Question with id '{questionId}' is not a roster size question.");
        }

        private IComposite GetGroupOrQuestionOrThrow(Guid groupOrQuestionId)
        {
            var groupOrQuestion = (IComposite)this.GetGroup(groupOrQuestionId) ?? this.GetQuestion(groupOrQuestionId);

            if (groupOrQuestion == null)
                throw new QuestionnaireException($"Group or question with id '{groupOrQuestionId}' is not found.");

            return groupOrQuestion;
        }

        private IGroup GetGroupOrThrow(Guid groupId, string customExceptionMessage = null)
        {
            IGroup group = this.GetGroup(groupId);

            if (group == null && groupId == innerDocument.PublicKey) group = this.innerDocument;

            if (group == null)
                throw new QuestionnaireException(customExceptionMessage ?? $"Group with id '{groupId}' is not found.");

            return group;
        }

        private IGroup GetGroup(Guid groupId) => GetGroup(this.GroupCache, groupId.FormatGuid());

        private IStaticText GetStaticTextOrThrow(Guid staticTextId)
        {
            IStaticText staticText = this.GetStaticTextImpl(staticTextId);

            if (staticText == null)
                throw new QuestionnaireException($"Static text with id '{staticTextId.FormatGuid()}' is not found.");

            return staticText;
        }

        private void ThrowIfQuestionDoesNotExist(Guid questionId) => this.GetQuestionOrThrow(questionId);

        private void ThrowIfEntityDoesNotExist(Guid entityId) => this.GetEntityOrThrow(entityId);

        private IQuestion GetQuestionOrThrow(Guid questionId) => GetQuestionOrThrow(this.QuestionCache, questionId);

        private IComposite GetEntityOrThrow(Guid entityId) => GetEntityOrThrow(this.EntityCache, entityId);

        private IQuestion GetQuestion(Guid questionId) => GetQuestion(this.QuestionCache, questionId);

        private IStaticText GetStaticTextImpl(Guid staticTextId) => GetEntity(this.EntityCache, staticTextId) as IStaticText;

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
        }

        private static IQuestion GetQuestionOrThrow(Dictionary<Guid, IQuestion> questions, Guid questionId)
        {
            IQuestion question = GetQuestion(questions, questionId);

            if (question == null)
                throw new QuestionnaireException($"Question with id '{questionId}' is not found.");

            return question;
        }

        private static IComposite GetEntityOrThrow(Dictionary<Guid, IComposite> entities, Guid entityId)
        {
            IComposite entity = GetEntity(entities, entityId);

            if (entity == null)
                throw new QuestionnaireException($"Entity with id '{entityId}' is not found.");

            return entity;
        }

        private static IQuestion GetQuestion(Dictionary<Guid, IQuestion> questions, Guid questionId) => questions.GetOrNull(questionId);

        private static IComposite GetEntity(Dictionary<Guid, IComposite> entities, Guid entityId) => entities.GetOrNull(entityId);

        private static IGroup GetGroup(Dictionary<string, IGroup> groups, string groupId) => groups.GetOrNull(groupId);

        private static IQuestion GetQuestionByStataCaption(Dictionary<Guid, IQuestion> questions, string identifier)
        {
            return questions.Values.FirstOrDefault(q => q.StataExportCaption == identifier);
        }
    }
}