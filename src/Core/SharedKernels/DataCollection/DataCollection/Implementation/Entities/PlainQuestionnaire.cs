using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.TopologicalSorter;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public class PlainQuestionnaire : IQuestionnaire
    {
        public ISubstitutionService SubstitutionService
        {
            get => this.substitutionService ??
                   (this.substitutionService = ServiceLocator.Current.GetInstance<ISubstitutionService>());
            set => this.substitutionService = value;
        }

        private ISubstitutionService substitutionService;

        public IQuestionOptionsRepository QuestionOptionsRepository => 
            this.questionOptionsRepository ?? 
            (this.questionOptionsRepository = ServiceLocator.Current.GetInstance<IQuestionOptionsRepository>());

        private IQuestionOptionsRepository questionOptionsRepository;

        #region State

        private readonly QuestionnaireDocument innerDocument;

        private IReadOnlyCollection<Guid> allRosterSizeQuestionsCache;
        private Dictionary<Guid, IVariable> variablesCache = null;
        private Dictionary<Guid, IStaticText> staticTextsCache = null;
        private Dictionary<Guid, IQuestion> questionsCache = null;
        private Dictionary<string, IQuestion> questionsByVariableCache = null;
        private Dictionary<Guid, IGroup> groupsCache = null;
        private Dictionary<Guid, IGroup> rostersCache = null;
        private Dictionary<Guid, IComposite> entitiesCache = null;
        private ReadOnlyCollection<Guid> sectionsCache = null;
        private Dictionary<string, HashSet<Guid>> substitutionReferencedQuestionsCache = null;
        private Dictionary<string, HashSet<Guid>> substitutionReferencedStaticTextsCache = null;
        private Dictionary<string, HashSet<Guid>> substitutionReferencedGroupsCache = null;
        private HashSet<string> questionVariableNamesCache = null;
        private HashSet<string> rosterVariableNamesCache = null;
        private HashSet<string> variableNamesCache = null;


        private readonly ConcurrentDictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingGroupsAndRosters = new ConcurrentDictionary<Guid, IEnumerable<Guid>>();
        private readonly ConcurrentDictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingGroups = new ConcurrentDictionary<Guid, IEnumerable<Guid>>();
        private readonly ConcurrentDictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingRosters = new ConcurrentDictionary<Guid, IEnumerable<Guid>>();
        private readonly ConcurrentDictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingQuestions = new ConcurrentDictionary<Guid, IEnumerable<Guid>>();
        private readonly ConcurrentDictionary<Guid, IEnumerable<Guid>> cacheOfRostersAffectedByRosterTitleQuestion = new ConcurrentDictionary<Guid, IEnumerable<Guid>>();

        private readonly ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>> cacheOfChildQuestions = new ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>>();
        private readonly ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>> cacheOfChildVariables = new ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>>();
        private readonly ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>> cacheOfChildEntities = new ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>>();
        private readonly ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>> cacheOfChildInterviewerQuestions = new ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>>();
        private readonly ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>> cacheOfUnderlyingInterviewerQuestions = new ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>>();
        private readonly ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>> cacheOfParentsStartingFromTop = new ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>>();
        private readonly ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>> cacheOfChildStaticTexts = new ConcurrentDictionary<Guid, ReadOnlyCollection<Guid>>();

        private readonly ConcurrentDictionary<Guid, ReadOnlyCollection<decimal>> cacheOfMultiSelectAnswerOptionsAsValues = new ConcurrentDictionary<Guid, ReadOnlyCollection<decimal>>();

        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<decimal, CategoricalOption>> cacheOfAnswerOptions = new ConcurrentDictionary<Guid, ConcurrentDictionary<decimal, CategoricalOption>>();
        private readonly ConcurrentDictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingStaticTexts = new ConcurrentDictionary<Guid, IEnumerable<Guid>>();


        public QuestionnaireDocument QuestionnaireDocument => this.innerDocument;

        public Guid? ResponsibleId => null;

        private readonly Translation translation;

        private Dictionary<Guid, IComposite> EntityCache
        {
            get
            {
                return this.entitiesCache ?? (this.entitiesCache
                    = this.innerDocument
                        .Find<IComposite>(_ => true)
                        .ToDictionary(
                            entity => entity.PublicKey,
                            entity => entity));
            }
        }

        private ReadOnlyCollection<Guid> SectionCache
        {
            get
            {
                return this.sectionsCache ?? (this.sectionsCache
                    = this.innerDocument
                    .Children
                    .OfType<IGroup>()
                    .Where(x => x != null)
                    .Select(x => x.PublicKey)
                    .ToReadOnlyCollection());
            }
        }

        private Dictionary<Guid, IVariable> VariablesCache
        {
            get
            {
                return this.variablesCache ?? (this.variablesCache
                    = this.innerDocument
                        .Find<IVariable>(_ => true)
                        .ToDictionary(
                            variable => variable.PublicKey,
                            variable => variable));
            }
        }

        private Dictionary<Guid, IStaticText> StaticTextCache
        {
            get
            {
                return this.staticTextsCache ?? (this.staticTextsCache
                    = this.innerDocument
                        .Find<IStaticText>(_ => true)
                        .ToDictionary(
                            staticText => staticText.PublicKey,
                            staticText => staticText));
            }
        }

        private Dictionary<Guid, IQuestion> QuestionCache
        {
            get
            {
                return this.questionsCache ?? (this.questionsCache
                    = this.innerDocument
                        .Find<IQuestion>(_ => true)
                        .ToDictionary(
                            question => question.PublicKey,
                            question => question));
            }
        }

        private Dictionary<string, IQuestion> QuestionsByVariableCache
            => this.questionsByVariableCache ?? (this.questionsByVariableCache
                   = this.innerDocument.Find<IQuestion>(_ => true)
                       .ToDictionary(question => question.VariableName.ToLower(), question => question));

        private Dictionary<Guid, IGroup> GroupCache
        {
            get
            {
                return this.groupsCache ?? (
                    this.groupsCache = this.innerDocument.Find<IGroup>(_ => true)
                        .ToDictionary(
                            group => group.PublicKey,
                            group => group));
            }
        }

        private Dictionary<Guid, IGroup> RosterCache
            => this.rostersCache ?? (this.rostersCache =
                   this.innerDocument.Find<IGroup>(IsRosterGroup)
                       .ToDictionary(group => @group.PublicKey, group => @group));

        private Dictionary<string, HashSet<Guid>> SubstitutionReferencedQuestionsCache
            => this.substitutionReferencedQuestionsCache
            ?? (this.substitutionReferencedQuestionsCache = this.GetSubstitutionReferencedQuestions());

        private Dictionary<string, HashSet<Guid>> SubstitutionReferencedStaticTextsCache
            => this.substitutionReferencedStaticTextsCache
            ?? (this.substitutionReferencedStaticTextsCache = this.GetSubstitutionReferencedStaticTexts());

        private Dictionary<string, HashSet<Guid>> SubstitutionReferencedGroupsCache
           => this.substitutionReferencedGroupsCache
           ?? (this.substitutionReferencedGroupsCache = this.GetSubstitutionReferencedGroups());

        private HashSet<string> QuestionVariableNamesCache
            => this.questionVariableNamesCache
               ?? (this.questionVariableNamesCache = this.GetQuestionVariableNamesCache());

        private HashSet<string> VariableNamesCache
            => this.variableNamesCache
               ?? (this.variableNamesCache = this.GetVariableNamesCache());

        private HashSet<string> RosterVariableNamesCache
            => this.rosterVariableNamesCache
               ?? (this.rosterVariableNamesCache = this.GetRosterNamesCache());

        private IEnumerable<IStaticText> AllStaticTexts => this.StaticTextCache.Values;

        private IEnumerable<IQuestion> AllQuestions => this.QuestionCache.Values;

        private IEnumerable<IVariable> AllVariables => this.VariablesCache.Values;

        private IEnumerable<IGroup> AllGroups => this.GroupCache.Values;

        #endregion

        public PlainQuestionnaire(QuestionnaireDocument document, long version, Translation translation = null)
        {
            this.innerDocument = document;
            this.Version = version;
            this.translation = translation;
        }

        public void WarmUpPriorityCaches()
        {
            var sections = this.SectionCache;
            var entities = this.EntityCache;
            var groups = this.GroupCache;
            var questions = this.QuestionCache;
            var staticTexts = this.StaticTextCache;
            var variables = this.VariablesCache;

            var substitutionReferencedQuestions = this.SubstitutionReferencedQuestionsCache;
            var substitutionReferencedStaticTexts = this.SubstitutionReferencedStaticTextsCache;
            var substitutionReferencedGroups = this.SubstitutionReferencedGroupsCache;
        }

        public long Version { get; }

        public Guid QuestionnaireId => this.innerDocument.PublicKey;

        public string Title => this.innerDocument.Title;

        public Translation Translation => this.translation;
        public string VariableName => this.innerDocument.VariableName;

        public IQuestion GetQuestionByVariable(string variable)
        {
            var questionId = this.GetQuestionIdByVariable(variable);
            return !questionId.HasValue ? null : this.GetQuestion(questionId.Value);
        }

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
                    return IsQuestionLinked(questionId) || IsQuestionLinkedToRoster(questionId)
                        ? AnswerType.RosterVector
                        : AnswerType.OptionCode;

                case QuestionType.MultyOption:
                    return
                        IsQuestionYesNo(questionId)
                            ? AnswerType.YesNoArray
                            : IsQuestionLinked(questionId) || IsQuestionLinkedToRoster(questionId)
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
                case QuestionType.Area:
                    return AnswerType.String;
                case QuestionType.Audio:
                    return AnswerType.FileName;
            }

            throw new ArgumentException($"Question of unknown type was found. Question id: {questionId}");
        }

        public bool IsQuestionLinked(Guid questionId) => this.GetQuestionOrThrow(questionId).LinkedToQuestionId.HasValue;

        public bool IsLinkedToListQuestion(Guid questionId)
        {
            var linkedQuestionId = this.GetQuestionOrThrow(questionId).LinkedToQuestionId;
            if (linkedQuestionId == null)
                return false;
            return this.GetQuestionOrThrow(linkedQuestionId.Value).QuestionType == QuestionType.TextList;
        }

        public Guid[] GetQuestionsLinkedToRoster()
        {
            return this.QuestionCache.Values.Where(x => x.LinkedToRosterId.HasValue).Select(x => x.PublicKey).ToArray();
        }

        public Guid[] GetQuestionsLinkedToQuestion()
        {
            return this.QuestionCache.Values.Where(x => x.LinkedToQuestionId.HasValue).Select(x => x.PublicKey).ToArray();
        }

        public Guid? GetQuestionIdByVariable(string variable)
        {
            variable = variable.ToLower();

            return this.QuestionsByVariableCache.ContainsKey(variable)
                ? this.QuestionsByVariableCache[variable].PublicKey
                : (Guid?) null;
        }

        public Guid GetVariableIdByVariableName(string variableName)
        {
            return this.VariablesCache.Values.Single(x => x.Name == variableName).PublicKey;
        }

        public Guid? GetRosterIdByVariableName(string variableName, bool ignoreCase = false)
            => this.RosterCache.Values.SingleOrDefault(x => string.Equals(x.VariableName, variableName,
                    ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
                ?.PublicKey;

        public string GetQuestionTitle(Guid questionId) => this.GetQuestionOrThrow(questionId).QuestionText;

        public string GetQuestionVariableName(Guid questionId) => this.GetQuestionOrThrow(questionId).StataExportCaption;

        public string GetGroupTitle(Guid groupId) => this.GetGroupOrThrow(groupId).Title;

        public string GetStaticText(Guid staticTextId) => this.GetStaticTextOrThrow(staticTextId).Text;

        public Attachment GetAttachmentForEntity(Guid entityId)
        {
            IStaticText staticText = this.GetStaticTextImpl(entityId);
            if (staticText == null)
                return null;

            var attachment = this.innerDocument.Attachments.SingleOrDefault(kv => kv.Name == staticText.AttachmentName);
            return attachment;
        }

        public Guid? GetCascadingQuestionParentId(Guid questionId) => this.GetQuestionOrThrow(questionId).CascadeFromQuestionId;

        public IEnumerable<decimal> GetMultiSelectAnswerOptionsAsValues(Guid questionId)
             => this.cacheOfMultiSelectAnswerOptionsAsValues.GetOrAdd(questionId, x
                => this.GetMultiSelectAnswerOptionsAsValuesImpl(questionId));
        
        public IEnumerable<CategoricalOption> GetOptionsForQuestion(Guid questionId, int? parentQuestionValue, string searchFor)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            CheckShouldQestionProvideOptions(question, questionId);

            //filtered and cascadings
            if (question.CascadeFromQuestionId.HasValue || (question.IsFilteredCombobox ?? false))
            {
                return QuestionOptionsRepository.GetOptionsForQuestion(new QuestionnaireIdentity(this.QuestionnaireId, Version),
                    questionId, parentQuestionValue, searchFor, this.translation);
            }

            //regular options
            return AnswerUtils.GetCategoricalOptionsFromQuestion(question, parentQuestionValue, searchFor);
        }

        public CategoricalOption GetOptionForQuestionByOptionText(Guid questionId, string optionText, int? parentQuestionValue)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            CheckShouldQestionProvideOptions(question, questionId);

            if (question.CascadeFromQuestionId.HasValue || (question.IsFilteredCombobox ?? false))
            {
                return QuestionOptionsRepository.GetOptionForQuestionByOptionText(
                    new QuestionnaireIdentity(this.QuestionnaireId, Version),
                    questionId,
                    optionText, 
                    parentQuestionValue,
                    this.translation);
            }

            return question.Answers.SingleOrDefault(x => x.AnswerText == optionText).ToCategoricalOption();
        }

        private ReadOnlyCollection<decimal> GetMultiSelectAnswerOptionsAsValuesImpl(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            CheckShouldQestionProvideOptions(question, questionId);

            return AnswerUtils.GetCategoricalOptionsFromQuestion(question, null, null)
                .Select(x => Convert.ToDecimal(x.Value)).ToReadOnlyCollection();
        }

        private void CheckShouldQestionProvideOptions(IQuestion question, Guid questionId)
        {
            bool questionTypeDoesNotSupportAnswerOptions
                = question.QuestionType != QuestionType.SingleOption && 
                  question.QuestionType != QuestionType.MultyOption && 
                  question.QuestionType != QuestionType.Numeric;

            if (questionTypeDoesNotSupportAnswerOptions)
                throw new QuestionnaireException(
                    $"Cannot return answer options for question with id '{questionId}' because it's type {question.QuestionType} does not support answer options.");
        }

        public CategoricalOption GetOptionForQuestionByOptionValueFromStructure(Guid questionId, decimal optionValue)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            CheckShouldQestionProvideOptions(question, questionId);

            return AnswerUtils.GetOptionForQuestionByOptionValue(question, optionValue);
        }

        public string GetAnswerOptionTitle(Guid questionId, decimal answerOptionValue) => this.GetAnswerOption(questionId, answerOptionValue).Title;

        public int GetCascadingParentValue(Guid questionId, decimal answerOptionValue)
        {
            var answerOption = this.GetAnswerOption(questionId, answerOptionValue);
            if (!answerOption.ParentValue.HasValue)
                throw new QuestionnaireException(
                    $"Answer option has no parent value. Option value: {answerOptionValue}, Question id: '{questionId}'.");

            return answerOption.ParentValue.Value;
        }

        private CategoricalOption GetAnswerOption(Guid questionId, decimal answerOptionValue)
            => this.cacheOfAnswerOptions.GetOrAdd(questionId, x => new ConcurrentDictionary<decimal, CategoricalOption>())
                        .GetOrAdd(answerOptionValue, GetAnswerOptionImpl(questionId, answerOptionValue));

        private CategoricalOption GetAnswerOptionImpl(Guid questionId, decimal optionValue)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            CheckShouldQestionProvideOptions(question, questionId);

            if (question.CascadeFromQuestionId.HasValue || (question.IsFilteredCombobox ?? false))
            {
                return QuestionOptionsRepository.GetOptionForQuestionByOptionValue(new QuestionnaireIdentity(this.QuestionnaireId, Version),
                    questionId, optionValue, this.translation);
            }

            return AnswerUtils.GetOptionForQuestionByOptionValue(question, optionValue);
        }

        public CategoricalOption GetOptionForQuestionByOptionValue(Guid questionId, decimal optionValue)
        {
            return GetAnswerOption(questionId, optionValue);
        }

        public IEnumerable<CategoricalOption> GetOptionsForQuestionFromStructure(Guid questionId, int? parentQuestionValue, string filter)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            CheckShouldQestionProvideOptions(question, questionId);

            return AnswerUtils.GetCategoricalOptionsFromQuestion(question, parentQuestionValue, filter);
        }

        public CategoricalOption GetOptionForQuestionByOptionTextFromStructure(Guid questionId, string optionText, int? parentQuestionValue)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            CheckShouldQestionProvideOptions(question, questionId);

            return question.Answers.SingleOrDefault(x => x.AnswerText == optionText && x.ParentCode == parentQuestionValue).ToCategoricalOption();
        }

        public int? GetMaxSelectedAnswerOptions(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            bool questionTypeDoesNotSupportMaxSelectedAnswerOptions = question.QuestionType != QuestionType.MultyOption && question.QuestionType != QuestionType.TextList;

            if (questionTypeDoesNotSupportMaxSelectedAnswerOptions || !(question is IMultyOptionsQuestion || question is TextListQuestion))
                throw new QuestionnaireException(
                    $"Cannot return maximum for selected answers for question with id '{questionId}' because it's type {question.QuestionType} does not support that parameter.");

            if (question is IMultyOptionsQuestion)
                return ((IMultyOptionsQuestion)question).MaxAllowedAnswers;

            return ((TextListQuestion)question).MaxAnswerCount;
        }

        public int GetMaxRosterRowCount() => Constants.MaxRosterRowCount;
        public int GetMaxLongRosterRowCount() => Constants.MaxLongRosterRowCount;

        public bool IsQuestion(Guid entityId) => this.HasQuestion(entityId);
        public bool IsStaticText(Guid entityId)
        {
            return this.GetStaticTextImpl(entityId) != null;
        }

        public bool IsInterviewierQuestion(Guid questionId)
        {
            var question = this.GetQuestion(questionId);

            return IsInterviewierQuestion(question);
        }

        private bool IsInterviewierQuestion(IQuestion question)
        {
            return question != null && question.QuestionScope == QuestionScope.Interviewer && !question.Featured;
        }

        public ReadOnlyCollection<Guid> GetPrefilledQuestions()
            => this
                .QuestionnaireDocument
                .Find<IQuestion>(question => question.Featured)
                .Select(question => question.PublicKey)
                .ToReadOnlyCollection();

        public ReadOnlyCollection<Guid> GetHiddenQuestions()
            => this
                .QuestionnaireDocument
                .Find<IQuestion>(question => question.QuestionScope == QuestionScope.Hidden)
                .Select(question => question.PublicKey)
                .ToReadOnlyCollection();
        

        public IEnumerable<Guid> GetAllParentGroupsForQuestion(Guid questionId) => this.GetAllParentGroupsForQuestionStartingFromBottom(questionId);

        public ReadOnlyCollection<Guid> GetParentsStartingFromTop(Guid entityId)
            => this.cacheOfParentsStartingFromTop.GetOrAdd(entityId,
                x => this.GetAllParentGroupsForEntityStartingFromBottom(entityId)
                    .Reverse()
                    .ToReadOnlyCollection());

        public Guid? GetParentGroup(Guid entityId)
        {
            var entity = this.GetEntityOrThrow(entityId);

            IComposite parent = entity.GetParent();

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

        public bool IsRosterSizeQuestion(Guid questionId)
        {
            return this.DoesQuestionSupportRoster(questionId)
                && this.GetRosterGroupsByRosterSizeQuestion(questionId).Any();
        }

        public IEnumerable<Guid> GetRosterGroupsByRosterSizeQuestion(Guid questionId)
        {
            if (!this.DoesQuestionSupportRoster(questionId))
                return Enumerable.Empty<Guid>();

            return this.AllGroups.Where(x => x.RosterSizeQuestionId == questionId && x.IsRoster).Select(x => x.PublicKey);
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

        public IEnumerable<Guid> GetRostersWithTitlesToChange()
        {
            return this.AllGroups
                .Where(x => x.IsRoster)
                .Where(x => x.RosterSizeSource == RosterSizeSourceType.FixedTitles ||
                            IsMultioptionQuestion(x.RosterSizeQuestionId) ||
                            (IsNumericQuestion(x.RosterSizeQuestionId) && x.RosterTitleQuestionId.HasValue))
                .Select(x => x.PublicKey)
                .ToList();
        }

        private bool IsMultioptionQuestion(Guid? questionId)
        {
            if (!questionId.HasValue) return false;
            return this.GetQuestion(questionId.Value)?.QuestionType == QuestionType.MultyOption;
        }

        private bool IsNumericQuestion(Guid? questionId)
        {
            if (!questionId.HasValue) return false;
            return this.GetQuestion(questionId.Value)?.QuestionType == QuestionType.Numeric;
        }

        public IEnumerable<Guid> GetFixedRosterGroups(Guid? parentRosterId = null)
        {
            if (parentRosterId.HasValue)
            {
                var nestedRosters = this.GetNestedRostersOfGroupById(parentRosterId.Value);
                return this.AllGroups
                    .Where(x => nestedRosters.Contains(x.PublicKey) && x.RosterSizeSource == RosterSizeSourceType.FixedTitles)
                    .Select(x => x.PublicKey)
                    .ToList();
            }

            return this.AllGroups
                .Where(x
                    => x.IsRoster
                    && x.RosterSizeSource == RosterSizeSourceType.FixedTitles
                    && this.GetRosterLevelForGroup(x.PublicKey) == 1)
                .Select(x => x.PublicKey)
                .ToList();
        }

        public IReadOnlyCollection<Guid> GetAllRosterSizeQuestions()
            => this.allRosterSizeQuestionsCache ?? (this.allRosterSizeQuestionsCache = this.innerDocument
                   .Find<IQuestion>(x => this.IsRosterSizeQuestion(x.PublicKey)).Select(x => x.PublicKey)
                   .ToReadOnlyCollection());

        private readonly ConcurrentDictionary<Guid, Guid[]> getRosterSizeSourcesCache = new ConcurrentDictionary<Guid, Guid[]>();
        public Guid[] GetRosterSizeSourcesForEntity(Guid entityId)
        {
            return getRosterSizeSourcesCache.GetOrAdd(entityId, id =>
            {
                var entity = GetEntityOrThrow(entityId);
                var rosterSizes = new List<Guid>();

                while (entity != this.innerDocument)
                {
                    if (entity is IGroup group)
                    {
                        if (IsRosterGroup(group))
                            rosterSizes.Insert(0, this.GetRosterSource(group.PublicKey));

                    }

                    entity = entity.GetParent();
                }

                return rosterSizes.ToArray();
            });
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

        public bool IsSubSection(Guid groupId)
        {
            IGroup @group = this.GetGroup(groupId);

            return @group?.IsRoster == false;
        }

        public bool IsVariable(Guid id)
            => this.AllVariables.Any(x => x.PublicKey == id);

        public ReadOnlyCollection<Guid> GetAllQuestions()
            => this.AllQuestions.Select(question => question.PublicKey).ToReadOnlyCollection();

        public
        ReadOnlyCollection<Guid> GetAllVariables()
              => this.AllVariables.Select(variable => variable.PublicKey).ToReadOnlyCollection();

        public ReadOnlyCollection<Guid> GetAllStaticTexts()
            => this.AllStaticTexts.Select(staticText => staticText.PublicKey).ToReadOnlyCollection();

        public ReadOnlyCollection<Guid> GetAllGroups()
            => this.AllGroups.Select(question => question.PublicKey).ToReadOnlyCollection();

        public ReadOnlyCollection<Guid> GetAllRosters()
            => this.AllGroups.Where(x => x.IsRoster).Select(question => question.PublicKey).ToReadOnlyCollection();

        public IEnumerable<Guid> GetAllUnderlyingQuestions(Guid groupId)
            => this.cacheOfUnderlyingQuestions.GetOrAdd(groupId, this.GetAllUnderlyingQuestionsImpl);

        public IEnumerable<Guid> GetAllUnderlyingStaticTexts(Guid groupId)
             => this.cacheOfUnderlyingStaticTexts.GetOrAdd(groupId, this.GetAllUnderlyingStaticTextsImpl);

        public ReadOnlyCollection<Guid> GetAllUnderlyingInterviewerQuestions(Guid groupId)
        {
            if (!this.cacheOfUnderlyingInterviewerQuestions.ContainsKey(groupId))
                this.cacheOfUnderlyingInterviewerQuestions[groupId] = this
                    .GetGroupOrThrow(groupId)
                    .Find<IQuestion>(IsInterviewierQuestion)
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

        private ReadOnlyCollection<Guid> GetChildVariables(Guid groupId)
        {
            if (!this.cacheOfChildVariables.ContainsKey(groupId))
            {
                this.cacheOfChildVariables[groupId] = new ReadOnlyCollection<Guid>(
                    this.GetGroupOrThrow(groupId)
                        .Children.OfType<IVariable>()
                        .Select(variable => variable.PublicKey)
                        .ToList());
            }

            return this.cacheOfChildVariables[groupId];
        }

        public IReadOnlyCollection<Guid> GetChildEntityIds(Guid groupId)
        {
            if (!this.cacheOfChildEntities.ContainsKey(groupId))
            {
                this.cacheOfChildEntities[groupId] =
                    this.GetGroupOrThrow(groupId)
                        .Children
                        //.Where(entity => !(entity is IVariable))
                        .Select(entity => entity.PublicKey)
                        .ToReadOnlyCollection();
            }

            return this.cacheOfChildEntities[groupId];
        }

        public IReadOnlyCollection<Guid> GetChildEntityIdsWithVariablesWithoutChache(Guid groupId)
        {
            return this.GetGroupOrThrow(groupId)
                .Children
                .Select(entity => entity.PublicKey)
                .ToReadOnlyCollection();
        }

        public IEnumerable<QuestionnaireItemReference> GetChidrenReferences(Guid groupId)
        {
            var group = this.GetGroupOrThrow(groupId);

            foreach (var child in group.Children)
            {
                var childEntityId = child.PublicKey;
                if (child is Group)
                {
                    if ((child as Group).IsRoster)
                        yield return new QuestionnaireItemReference(QuestionnaireReferenceType.Roster, child.PublicKey);
                    else
                        yield return new QuestionnaireItemReference(QuestionnaireReferenceType.SubSection, childEntityId);
                }
                else if (child is IStaticText)
                {
                    yield return new QuestionnaireItemReference(QuestionnaireReferenceType.StaticText, childEntityId);
                }
                else if (child is IQuestion)
                {
                    yield return new QuestionnaireItemReference(QuestionnaireReferenceType.Question, childEntityId);
                }
                else if (child is IVariable)
                {
                    yield return new QuestionnaireItemReference(QuestionnaireReferenceType.Variable, childEntityId);
                }
            }
        }

        public Guid? GetCommonParentRosterForLinkedQuestionAndItSource(Guid linkedQuestionId)
        {
            var isQuestionLinkedToQuestion = this.IsQuestionLinked(linkedQuestionId);
            var questionSourceId = isQuestionLinkedToQuestion
                  ? this.GetQuestionReferencedByLinkedQuestion(linkedQuestionId)
                  : this.GetRosterReferencedByLinkedQuestion(linkedQuestionId);

            var linkedSourceRosterScopes = this.GetRosterSizeSourcesForEntity(questionSourceId).Shrink();
            var linkedRosterScopes = this.GetRosterSizeSourcesForEntity(linkedQuestionId);

            var mutualRosterSizeSources = linkedSourceRosterScopes.GetCommonPart(linkedRosterScopes).ToList();

            if (!mutualRosterSizeSources.Any())
                return null;

            var closestRosterSizeSource = mutualRosterSizeSources.Last();

            var targetRostersWithSameScope = new List<Guid>(IsFixedRoster(closestRosterSizeSource)
                ? closestRosterSizeSource.ToEnumerable()
                : GetRosterGroupsByRosterSizeQuestion(closestRosterSizeSource));

            if (!targetRostersWithSameScope.Any())
                return null;

            Guid targetRoster;
            targetRoster = targetRostersWithSameScope.Count == 1
                ? targetRostersWithSameScope.First()
                : targetRostersWithSameScope.FirstOrDefault(rosterId => isQuestionLinkedToQuestion 
                    ? IsQuestionChildOfGroup(questionSourceId, rosterId)
                    : IsRosterChildOfGroupRecursive(questionSourceId, rosterId));

            if (targetRoster == Guid.Empty) return null;

            return targetRoster;
        }

        public string GetVariableLabel(Guid variableId) => this.GetVariable(variableId).Label;

        public string GetVariableName(Guid variableId) => this.GetVariable(variableId).Name;
        public string GetRosterVariableName(Guid id) => this.GetGroupOrThrow(id).VariableName;

        public IReadOnlyCollection<int> GetValidationWarningsIndexes(Guid entityId)
        {
            return GetEntityOrThrow(entityId).GetValidationConditions()
                .Select((v, i) => new {index = i, validationCondition = v})
                .Where(v => v.validationCondition.Severity == ValidationSeverity.Warning)
                .Select(v => v.index)
                .ToReadOnlyCollection();
        }

        public bool IsSignature(Guid entityIdentityId)
        {
            var question = this.GetQuestion(entityIdentityId) as IMultimediaQuestion;
            return question?.IsSignature ?? false;
        }

        public bool IsRosterTriggeredByOrderedMultiQuestion(Guid rosterId)
        {
            if (!IsRosterGroup(rosterId))
                return false;

            if (IsFixedRoster(rosterId))
                return false;

            var rosterSizeQuestionId = GetRosterSizeQuestion(rosterId);

            return ShouldQuestionRecordAnswersOrder(rosterSizeQuestionId);
        }

        public bool HasVariable(Guid variableId) => this.GetVariable(variableId) != null;
        public bool HasStaticText(Guid entityId) => this.GetStaticText(entityId) != null;

        private bool IsQuestionChildOfGroup(Guid questionId, Guid groupId)
        {
            return GetAllUnderlyingQuestions(groupId).Contains(questionId);
        }

        private bool IsRosterChildOfGroupRecursive(Guid rosterId, Guid groupId)
        {
            if (rosterId == groupId) return true;
            var group = this.GetGroup(rosterId);
            return group.UnwrapReferences<IComposite>(x => x.GetParent()).Any(x => x.PublicKey == groupId);
        }

        public IReadOnlyList<Guid> GetAllUnderlyingInterviewerEntities(Guid groupId)
        {
            var result = GetChildEntityIds(groupId)
                        .Except(x => (this.IsQuestion(x) && !this.IsInterviewierQuestion(x)));

            return new ReadOnlyCollection<Guid>(result.ToList());
        }

        public IReadOnlyList<Guid> GetAllUnderlyingEntities(Guid groupId)
        {
            var result = GetChildEntityIds(groupId);
            return new ReadOnlyCollection<Guid>(result.ToList());
        }

        public IReadOnlyList<Guid> GetSubSectionsWithEnablementCondition(Guid groupId)
        {
            var result = GetChildEntityIds(groupId)
                .Where(x => this.IsSubSection(x) && !string.IsNullOrWhiteSpace(this.GetCustomEnablementConditionForGroup(x)));

            return new ReadOnlyCollection<Guid>(result.ToList());
        }
         

        public ReadOnlyCollection<Guid> GetChildInterviewerQuestions(Guid groupId)
            => this.cacheOfChildInterviewerQuestions.GetOrAdd(groupId, this
                    .GetGroupOrThrow(groupId)
                    .Children.OfType<IQuestion>()
                    .Where(IsInterviewierQuestion)
                    .Select(question => question.PublicKey)
                    .ToReadOnlyCollection());

        public ReadOnlyCollection<Guid> GetChildStaticTexts(Guid groupId)
             => this.cacheOfChildStaticTexts.GetOrAdd(groupId, x
                => this
                    .GetGroupOrThrow(groupId)
                    .Children.OfType<IStaticText>()
                    .Select(s => s.PublicKey)
                    .ToReadOnlyCollection());


        public bool IsPrefilled(Guid questionId)
        {
            var question = this.GetQuestionOrThrow(questionId);
            return question.Featured;
        }

        public bool ShouldBeHiddenIfDisabled(Guid entityId)
        {
            IComposite entity = this.GetEntityOrThrow(entityId);

            var question = entity as IQuestion;
            if (question?.CascadeFromQuestionId != null)
            {
                return this.ShouldBeHiddenIfDisabled(question.CascadeFromQuestionId.Value);
            }

            return (entity as IConditional)?.HideIfDisabled ?? false;
        }

        public string GetValidationMessage(Guid questionId, int conditionIndex)
        {
            if (IsQuestion(questionId))
            {
                return this.GetQuestion(questionId).ValidationConditions[conditionIndex].Message;
            }
            else if (IsStaticText(questionId))
            {
                return this.GetStaticTextImpl(questionId).ValidationConditions[conditionIndex].Message;
            }

            return null;
        }

        public string[] GetValidationMessages(Guid entityId)
        {
            if (IsQuestion(entityId))
            {
                return this.GetQuestion(entityId).ValidationConditions.Select(x => x.Message).ToArray();
            }
            else if (IsStaticText(entityId))
            {
                return this.GetStaticTextImpl(entityId).ValidationConditions.Select(x => x.Message).ToArray();
            }

            return null;
        }

        public bool HasMoreThanOneValidationRule(Guid questionId)
        {
            if (this.IsQuestion(questionId))
            {
                return this.GetQuestion(questionId).ValidationConditions.Count > 1;
            }

            if (this.IsStaticText(questionId))
            {
                return this.GetStaticTextImpl(questionId).ValidationConditions.Count > 1;
            }

            return false;
        }

        public string GetQuestionInstruction(Guid questionId)
        {
            return this.GetQuestion(questionId).Instructions;
        }

        public bool IsQuestionFilteredCombobox(Guid questionId)
        {
            var singleQuestion = (this.GetQuestion(questionId) as SingleQuestion);
            return singleQuestion?.IsFilteredCombobox ?? false;
        }

        public bool IsQuestionCascading(Guid questionId)
        {
            var singleQuestion = (this.GetQuestion(questionId) as SingleQuestion);
            return singleQuestion?.CascadeFromQuestionId.HasValue ?? false;
        }

        public bool ShouldQuestionRecordAnswersOrder(Guid questionId)
        {
            var multiOptionsQuestion = (this.GetQuestion(questionId) as IMultyOptionsQuestion);
            return multiOptionsQuestion?.AreAnswersOrdered == true;
        }

        public string GetTextQuestionMask(Guid questionId)
        {
            var textQuestion = (this.GetQuestion(questionId) as TextQuestion);
            return textQuestion?.Mask;
        }

        public bool GetHideInstructions(Guid questionId)
        {
            return this.GetQuestion(questionId).Properties.HideInstructions;
        }

        public bool ShouldUseFormatting(Guid questionId)
        {
            var numericQuestion = this.GetQuestion(questionId) as INumericQuestion;
            return numericQuestion?.UseFormatting ?? false;
        }

        public bool HasVariable(string variableName)
        {
            return this.VariableNamesCache.Contains(variableName);
        }

        public bool HasQuestion(string variableName)
        {
            variableName = variableName.ToLower();

            return this.QuestionVariableNamesCache.Contains(variableName);
        }

        public bool HasRoster(string variableName) => this.RosterVariableNamesCache.Contains(variableName?.ToLower());

        public bool IsTimestampQuestion(Guid questionId) => (this.GetQuestion(questionId) as DateTimeQuestion)?.IsTimestamp ?? false;
        public bool IsSupportFilteringForOptions(Guid questionId)
        {
            return !this.GetQuestion(questionId).Properties.OptionsFilterExpression?.Trim().IsNullOrEmpty() ?? false;
        }

        public bool IsFixedRoster(Guid id)
        {
            var @group = this.GetGroup(id);
            return @group != null && @group.RosterSizeSource == RosterSizeSourceType.FixedTitles;
        }

        public decimal[] GetFixedRosterCodes(Guid rosterId) =>
            this.GetGroup(rosterId)?.FixedRosterTitles?.Select(x => x.Value)?.ToArray() ?? Array.Empty<decimal>();

        public bool IsNumericRoster(Guid id)
        {
            var @group = this.GetGroup(id);
            return @group != null && @group.RosterSizeQuestionId != null && this.GetQuestion(@group.RosterSizeQuestionId.Value)?.QuestionType == QuestionType.Numeric;
        }

        public IReadOnlyCollection<string> GetTranslationLanguages()
            => this
                .QuestionnaireDocument
                .Translations
                .Select(translation => translation.Name)
                .ToReadOnlyCollection();

        public string GetDefaultTransation()
        {
            return this.QuestionnaireDocument.Translations.SingleOrDefault(t =>
                t.Id == this.QuestionnaireDocument.DefaultTranslation)?.Name;
        }

        public bool IsQuestionIsRosterSizeForLongRoster(Guid questionId)
        {
            var rosters = GetRosterGroupsByRosterSizeQuestion(questionId).ToArray();
            if (!rosters.Any())
                return false;

            int childItemsCount = 0;

            foreach (var roster in rosters)
            {
                if (GetRosterLevelForEntity(roster) > 1)
                    return false;
                if (GetAllUnderlyingChildRosters(roster).Any())
                    return false;

                childItemsCount += GetGroupOrThrow(roster).Children.TreeToEnumerable(x => x.Children).Count();

                if (childItemsCount > Constants.MaxAmountOfItemsInLongRoster)
                    return false;
            }

            return true;
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
                throw new QuestionnaireException($"Cannot return id of referenced question because specified question {FormatQuestionForException(linkedQuestion)} is not linked.");

            return linkedQuestion.LinkedToQuestionId.Value;
        }

        public Guid GetRosterReferencedByLinkedQuestion(Guid linkedQuestionId)
        {
            IQuestion linkedQuestion = this.GetQuestionOrThrow(linkedQuestionId);

            if (!linkedQuestion.LinkedToRosterId.HasValue)
                throw new QuestionnaireException($"Cannot return id of referenced roster because specified question {FormatQuestionForException(linkedQuestion)} is not linked.");

            return linkedQuestion.LinkedToRosterId.Value;
        }

        public bool IsQuestionLinkedToRoster(Guid linkedQuestionId)
        {
            IQuestion linkedQuestion = this.GetQuestionOrThrow(linkedQuestionId);

            return linkedQuestion.LinkedToRosterId.HasValue;
        }

        public bool IsQuestionInteger(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            var numericQuestion = question as INumericQuestion;
            if (numericQuestion == null)
                return false;
            //throw new QuestionnaireException(string.Format("Question with id '{0}' must be numeric.", questionId));

            return numericQuestion.IsInteger;
        }

        public bool IsQuestionYesNo(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            var multipleOptionsQuestion = question as IMultyOptionsQuestion;
            if (multipleOptionsQuestion == null)
                return false;
            //throw new QuestionnaireException($"Question with id '{questionId}' must be multiple options question.");

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

        public string GetFixedRosterTitle(Guid groupId, decimal fixedTitleValue)
            => this.GetFixedRosterTitles(groupId).Single(title => title.Value == fixedTitleValue).Title;

        public bool DoesQuestionSpecifyRosterTitle(Guid questionId) => this.GetRostersAffectedByRosterTitleQuestion(questionId).Any();

        public IEnumerable<Guid> GetRostersAffectedByRosterTitleQuestion(Guid questionId)
        {
            return this.cacheOfRostersAffectedByRosterTitleQuestion.GetOrAdd(questionId,
                this.GetRostersAffectedByRosterTitleQuestionImpl(questionId));
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

        public Guid GetRosterSizeQuestion(Guid rosterId)
        {
            var roster = this.GetGroupOrThrow(rosterId);
            return roster.RosterSizeQuestionId.Value;
        }

        public Guid? GetRosterTitleQuestionId(Guid rosterId)
        {
            var roster = this.GetGroupOrThrow(rosterId);
            return roster.RosterTitleQuestionId;
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
            return questionsCache.Values.Where(x =>
            {
                var question = x as AbstractQuestion;
                return question != null && question.CascadeFromQuestionId.HasValue && question.CascadeFromQuestionId.Value == id;
            }).Select(x => x.PublicKey);
        }

        public IEnumerable<Guid> GetAllChildCascadingQuestions()
        {
            return questionsCache.Values.Where(x =>
            {
                var question = x as AbstractQuestion;
                return question != null && question.CascadeFromQuestionId.HasValue;
            }).Select(x => x.PublicKey);
        }

        private Dictionary<string, HashSet<Guid>> GetSubstitutionReferencedQuestions()
        {
            return this.GetSubstitutionReferencedEntities(this.AllQuestions);
        }

        private Dictionary<string, HashSet<Guid>> GetSubstitutionReferencedStaticTexts()
        {
            return this.GetSubstitutionReferencedEntities(this.AllStaticTexts);
        }

        private Dictionary<string, HashSet<Guid>> GetSubstitutionReferencedGroups()
        {
            return this.GetSubstitutionReferencedEntities(this.AllGroups);
        }

        private HashSet<string> GetQuestionVariableNamesCache() 
            => this.AllQuestions.Select(x => x.StataExportCaption.ToLower()).ToHashSet();

        private HashSet<string> GetVariableNamesCache()
        {
            return this.AllVariables.Select(x => x.Name).ToHashSet();
        }

        private HashSet<string> GetRosterNamesCache() =>
            this.AllGroups.Where(x => x.IsRoster).Select(x => x.VariableName.ToLower()).ToHashSet();

        private Dictionary<string, HashSet<Guid>> GetSubstitutionReferencedEntities(IEnumerable<IComposite> entities)
        {
            var referenceOccurences = new Dictionary<string, HashSet<Guid>>();
            foreach (IComposite entity in entities)
            {
                var substitutedVariableNames = this.SubstitutionService.GetAllSubstitutionVariableNames(entity.GetTitle()).ToList();
                var validateable = entity as IValidatable;
                if (validateable != null)
                {
                    foreach (ValidationCondition validationCondition in validateable.ValidationConditions)
                    {
                        var substitutedVariablesInValidation = this.SubstitutionService.GetAllSubstitutionVariableNames(validationCondition.Message);
                        substitutedVariableNames.AddRange(substitutedVariablesInValidation);
                    }
                }

                foreach (var substitutedVariableName in substitutedVariableNames)
                {
                    if (!referenceOccurences.ContainsKey(substitutedVariableName))
                    {
                        referenceOccurences.Add(substitutedVariableName, new HashSet<Guid>());
                    }
                    if (!referenceOccurences[substitutedVariableName].Contains(entity.PublicKey))
                    {
                        referenceOccurences[substitutedVariableName].Add(entity.PublicKey);
                    }
                }
            }

            return referenceOccurences;
        }

        public IEnumerable<Guid> GetSubstitutedQuestions(Guid questionId)
        {
            string targetVariableName = this.GetQuestionVariableName(questionId);

            if (!string.IsNullOrWhiteSpace(targetVariableName))
            {
                if (this.SubstitutionReferencedQuestionsCache.ContainsKey(targetVariableName))
                {
                    foreach (var referencingQuestionId in this.SubstitutionReferencedQuestionsCache[targetVariableName])
                    {
                        yield return referencingQuestionId;
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

        public IEnumerable<Guid> GetSubstitutedStaticTexts(Guid questionId)
        {
            string targetVariableName = this.GetQuestionVariableName(questionId);

            if (!string.IsNullOrWhiteSpace(targetVariableName))
            {
                if (this.SubstitutionReferencedStaticTextsCache.ContainsKey(targetVariableName))
                {
                    foreach (var referencingStaticTextId in this.SubstitutionReferencedStaticTextsCache[targetVariableName])
                    {
                        yield return referencingStaticTextId;
                    }
                }
            }

            var rostersAffectedByRosterTitleQuestion = this.GetRostersAffectedByRosterTitleQuestion(questionId);
            foreach (var rosterId in rostersAffectedByRosterTitleQuestion)
            {
                IEnumerable<Guid> staticTextsInRoster = this.GetAllUnderlyingStaticTexts(rosterId);
                foreach (var staticTextInRosterId in staticTextsInRoster)
                {
                    var staticTextTitle = GetStaticText(staticTextInRosterId);
                    if (this.SubstitutionService.ContainsRosterTitle(staticTextTitle))
                    {
                        yield return staticTextInRosterId;
                    }
                }
            }
        }

        public IEnumerable<Guid> GetSubstitutedGroups(Guid questionId)
        {
            string targetVariableName = this.GetQuestionVariableName(questionId);

            if (!string.IsNullOrWhiteSpace(targetVariableName))
            {
                if (this.SubstitutionReferencedGroupsCache.ContainsKey(targetVariableName))
                {
                    foreach (var referencingGroupId in this.SubstitutionReferencedGroupsCache[targetVariableName])
                    {
                        yield return referencingGroupId;
                    }
                }
            }

            var rostersAffectedByRosterTitleQuestion = this.GetRostersAffectedByRosterTitleQuestion(questionId);
            foreach (var rosterId in rostersAffectedByRosterTitleQuestion)
            {
                IEnumerable<Guid> groupsInRoster = this.GetAllUnderlyingChildGroups(rosterId);
                foreach (var groupId in groupsInRoster)
                {
                    var groupTitle = GetGroupTitle(groupId);
                    if (this.SubstitutionService.ContainsRosterTitle(groupTitle))
                    {
                        yield return groupId;
                    }
                }
            }
        }

        public IEnumerable<Guid> GetAllSections() => this.SectionCache;

        private IEnumerable<Guid> GetRostersAffectedByRosterTitleQuestionImpl(Guid questionId)
        {
            IEnumerable<Guid> rostersAffectedByCurrentDomain =
                from @group in this.AllGroups
                where this.IsRosterGroup(@group.PublicKey) && @group.RosterTitleQuestionId == questionId
                select @group.PublicKey;

            return rostersAffectedByCurrentDomain.ToList();
        }

        private IEnumerable<Guid> GetAllUnderlyingStaticTextsImpl(Guid groupId)
            => this
                .GetGroupOrThrow(groupId)
                .Find<IStaticText>(_ => true)
                .Select(question => question.PublicKey)
                .ToList();

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

        private IGroup GetGroupOrThrow(Guid groupId, string customExceptionMessage = null)
        {
            IGroup group = this.GetGroup(groupId);

            if (group == null && groupId == innerDocument.PublicKey) group = this.innerDocument;

            if (group == null)
                throw new QuestionnaireException(customExceptionMessage ?? $"Group with id '{groupId}' is not found.");

            return group;
        }

        private IGroup GetGroup(Guid groupId) => GetGroup(this.GroupCache, groupId);

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

        private IVariable GetVariable(Guid questionId) => GetVariable(this.VariablesCache, questionId);

        private IStaticText GetStaticTextImpl(Guid staticTextId) => GetEntity(this.EntityCache, staticTextId) as IStaticText;

        private static string FormatQuestionForException(IQuestion question)
        {
            return string.Format("'{0} [{1}] ({2:N})'",
                question.QuestionText ?? "<<NO QUESTION TITLE>>",
                question.StataExportCaption ?? "<<NO VARIABLE NAME>>",
                question.PublicKey);
        }

        private static IQuestion GetQuestionOrThrow(Dictionary<Guid, IQuestion> questions, Guid questionId)
        {
            IQuestion question = GetQuestion(questions, questionId);

            if (question == null)
                throw new QuestionnaireException("Question is not found.")
                {
                    Data = {{"QuestionId", questionId}}
                };

            return question;
        }

        private static IComposite GetEntityOrThrow(Dictionary<Guid, IComposite> entities, Guid entityId)
        {
            IComposite entity = GetEntity(entities, entityId);

            if (entity == null)
                throw new QuestionnaireException($"Entity is not found.")
                {
                    Data = {{"entityId", entityId}}
                };

            return entity;
        }

        private static IQuestion GetQuestion(Dictionary<Guid, IQuestion> questions, Guid questionId) => questions.GetOrNull(questionId);

        private static IVariable GetVariable(Dictionary<Guid, IVariable> variables, Guid variableId) => variables.GetOrNull(variableId);

        private static IComposite GetEntity(Dictionary<Guid, IComposite> entities, Guid entityId) => entities.GetOrNull(entityId);

        private static IGroup GetGroup(Dictionary<Guid, IGroup> groups, Guid groupId) => groups.GetOrNull(groupId);

        public Guid GetFirstSectionId()
        {
            return this.GetAllSections().First();
        }

        public IEnumerable<Guid> GetLinkedToSourceEntity(Guid linkedSourceEntityId)
        {
            return this.AllQuestions.Where(
                    x => (x.LinkedToQuestionId == linkedSourceEntityId)
                      || (x.LinkedToRosterId == linkedSourceEntityId))
                    .Select(x => x.PublicKey);
        }

        public bool IsUsingExpressionStorage()
        {
            return this.QuestionnaireDocument.IsUsingExpressionStorage;
        }

        public List<Guid> GetExpressionsPlayOrder()
        {
            return this.QuestionnaireDocument.ExpressionsPlayOrder;
        }

        public bool SupportsExpressionsGraph()
        {
            return this.QuestionnaireDocument.DependencyGraph != null;
        }

        public List<Guid> GetExpressionsPlayOrder(Guid changedEntity)
        {
            var sorter = new TopologicalSorter<Guid>();
            IEnumerable<Guid> lisOsfOrderedConditions = sorter.Sort(this.QuestionnaireDocument.DependencyGraph, changedEntity);
            return lisOsfOrderedConditions.ToList();
        }

        public List<Guid> GetValidationExpressionsPlayOrder(IEnumerable<Guid> entities)
        {
            if (IsUsingExpressionStorage())
            {
                HashSet<Guid> entityIds = new HashSet<Guid>();
                foreach (var entity in entities)
                {
                    entityIds.Add(entity);

                    if (this.QuestionnaireDocument.ValidationDependencyGraph.TryGetValue(entity, out Guid[] referancecs))
                        referancecs.ForEach(id => entityIds.Add(id));
                }

                return entityIds.ToList();
            }

            return this.QuestionnaireDocument.ExpressionsPlayOrder;
        }

        public bool HasAnyCascadingOptionsForSelectedParentOption(Guid cascadingQuestionId, Guid parenQuestionId,
            int selectedParentValue)
        {
            var options = this.GetOptionsForQuestion(cascadingQuestionId, selectedParentValue, string.Empty);
            return options.Any();
        }

        public IEnumerable<Guid> GetAllUnderlyingQuestionsOutsideRosters(Guid? groupId)
        {
            var groupOrQuestionnaireId = groupId ?? this.QuestionnaireId;
            foreach (var questionId in this.GetChildQuestions(groupOrQuestionnaireId))
                yield return questionId;

            foreach (var subGroupId in this.GetAllUnderlyingChildGroups(groupOrQuestionnaireId))
            foreach (var questionId in this.GetAllUnderlyingQuestionsOutsideRosters(subGroupId))
                yield return questionId;
        }

        public IEnumerable<Guid> GetAllUnderlyingVariablesOutsideRosters(Guid? groupId)
        {
            var groupOrQuestionnaireId = groupId ?? this.QuestionnaireId;

            foreach (var variableId in this.GetChildVariables(groupOrQuestionnaireId))
                yield return variableId;

            foreach (var subGroupId in this.GetAllUnderlyingChildGroups(groupOrQuestionnaireId))
            foreach (var variableId in this.GetAllUnderlyingVariablesOutsideRosters(subGroupId))
                yield return variableId;
        }
    }
}
