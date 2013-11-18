using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Questionnaire;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class Questionnaire : AggregateRootMappedByConvention, IQuestionnaire, ISnapshotable<QuestionnaireState>
    {
        #region State

        private QuestionnaireDocument innerDocument = new QuestionnaireDocument();
        private Dictionary<Guid, IQuestion> questionCache = null;
        private Dictionary<Guid, IGroup> groupCache = null;
        private Dictionary<Guid, IEnumerable<QuestionIdAndVariableName>> cacheOfQuestionsInvolvedInCustomValidationOfQuestion = new Dictionary<Guid, IEnumerable<QuestionIdAndVariableName>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfQuestionsWhichCustomValidationDependsOnQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<QuestionIdAndVariableName>> cacheOfQuestionsInvolvedInCustomEnablementConditionOfGroup = new Dictionary<Guid, IEnumerable<QuestionIdAndVariableName>>();
        private Dictionary<Guid, IEnumerable<QuestionIdAndVariableName>> cacheOfQuestionsInvolvedInCustomEnablementConditionOfQuestion = new Dictionary<Guid, IEnumerable<QuestionIdAndVariableName>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfGroupsWhichCustomEnablementConditionDependsOnQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfQuestionsWhichCustomEnablementConditionDependsOnQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingQuestions = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingQuestionsWithNotEmptyCustomEnablementConditions = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingQuestionsWithNotEmptyCustomValidationExpressions = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingMandatoryQuestions = new Dictionary<Guid, IEnumerable<Guid>>();

        protected internal void OnTemplateImported(TemplateImported e)
        {
            this.innerDocument = e.Source;
            this.innerDocument.ConnectChildrenWithParent();

            this.questionCache = null;
            this.groupCache = null;
            this.cacheOfQuestionsInvolvedInCustomValidationOfQuestion = new Dictionary<Guid, IEnumerable<QuestionIdAndVariableName>>();
            this.cacheOfQuestionsWhichCustomValidationDependsOnQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfQuestionsInvolvedInCustomEnablementConditionOfGroup = new Dictionary<Guid, IEnumerable<QuestionIdAndVariableName>>();
            this.cacheOfQuestionsInvolvedInCustomEnablementConditionOfQuestion = new Dictionary<Guid, IEnumerable<QuestionIdAndVariableName>>();
            this.cacheOfGroupsWhichCustomEnablementConditionDependsOnQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfQuestionsWhichCustomEnablementConditionDependsOnQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfUnderlyingQuestions = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfUnderlyingQuestionsWithNotEmptyCustomEnablementConditions = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfUnderlyingQuestionsWithNotEmptyCustomValidationExpressions = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfUnderlyingMandatoryQuestions = new Dictionary<Guid, IEnumerable<Guid>>();
        }

        public QuestionnaireState CreateSnapshot()
        {
            return new QuestionnaireState(this.innerDocument, this.questionCache, this.groupCache);
        }

        public void RestoreFromSnapshot(QuestionnaireState snapshot)
        {
            this.innerDocument = snapshot.Document;
            this.groupCache = snapshot.GroupCache;
            this.questionCache = snapshot.QuestionCache;
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

        private Dictionary<Guid, IGroup> GroupCache
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

        #region Dependencies

        private ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        /// <remarks>
        /// All operations with expressions are time-consuming.
        /// So this processor may be used only in command handlers or in domain methods.
        /// And should never be used in event handlers!!
        /// </remarks>
        private static IExpressionProcessor ExpressionProcessor
        {
            get { return ServiceLocator.Current.GetInstance<IExpressionProcessor>(); }
        }

        private static IQuestionnaireVerifier QuestionnaireVerifier
        {
            get { return ServiceLocator.Current.GetInstance<IQuestionnaireVerifier>(); }
        }

        #endregion


        public Questionnaire(){}

        public Questionnaire(Guid createdBy, IQuestionnaireDocument source)
            : base(source.PublicKey)
        {
            this.ImportFromDesigner(createdBy, source);
        }

        public Questionnaire(IQuestionnaireDocument source)
            : base(source.PublicKey)
        {
            this.ImportFromSupervisor(source);
        }


        public void ImportFromDesigner(Guid createdBy, IQuestionnaireDocument source)
        {
            QuestionnaireDocument document = CastToQuestionnaireDocumentOrThrow(source);
            document.ConnectChildrenWithParent();

            this.ThrowIfVerifierFindsErrors(document);
            
            document.CreatedBy = this.innerDocument.CreatedBy;

            this.ApplyEvent(new TemplateImported() {Source = document});
        }

        public void ImportFromSupervisor(IQuestionnaireDocument source)
        {
            QuestionnaireDocument document = CastToQuestionnaireDocumentOrThrow(source);
            this.ApplyEvent(new TemplateImported() { Source = document });
        }

        public void ImportFromDesignerForTester(IQuestionnaireDocument source)
        {
            QuestionnaireDocument document = CastToQuestionnaireDocumentOrThrow(source);
            this.ApplyEvent(new TemplateImported() { Source = document });
        }

        public IQuestion GetQuestionByStataCaption(string stataCaption)
        {
            return this.innerDocument.FirstOrDefault<IQuestion>(q => q.StataExportCaption == stataCaption);
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

        public Guid? GetQuestionLinkedQuestionId(Guid questionId)
        {
            return this.GetQuestionOrThrow(questionId).LinkedToQuestionId;
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

        public int? GetMaxSelectedAnswerOptions(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            bool questionTypeDoesNotSupportMaxSelectedAnswerOptions = question.QuestionType != QuestionType.MultyOption;

            if (questionTypeDoesNotSupportMaxSelectedAnswerOptions || !(question is IMultyOptionsQuestion))
                throw new QuestionnaireException(string.Format(
                    "Cannot return maximum for selected answers for question with id '{0}' because it's type {1} does not support that parameter.",
                    questionId, question.QuestionType));

            return ((IMultyOptionsQuestion) question).MaxAllowedAnswers;
        }

        public bool IsCustomValidationDefined(Guid questionId)
        {
            var validationExpression = this.GetCustomValidationExpression(questionId);

            return IsExpressionDefined(validationExpression);
        }

        public IEnumerable<QuestionIdAndVariableName> GetQuestionsInvolvedInCustomValidation(Guid questionId)
        {
            if (!this.cacheOfQuestionsInvolvedInCustomValidationOfQuestion.ContainsKey(questionId))
                this.cacheOfQuestionsInvolvedInCustomValidationOfQuestion[questionId]
                    = this.GetQuestionsInvolvedInCustomValidationImpl(questionId);

            return this.cacheOfQuestionsInvolvedInCustomValidationOfQuestion[questionId];
        }

        public IEnumerable<Guid> GetAllQuestionsWithNotEmptyValidationExpressions()
        {
            return
              from question in this.GetAllQuestions()
              where IsExpressionDefined(question.ValidationExpression)
              select question.PublicKey;
        }

        public string GetCustomValidationExpression(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            return question.ValidationExpression;
        }

        public IEnumerable<Guid> GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(Guid questionId)
        {
            if (!this.cacheOfQuestionsWhichCustomValidationDependsOnQuestion.ContainsKey(questionId))
                this.cacheOfQuestionsWhichCustomValidationDependsOnQuestion[questionId]
                    = this.GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestionImpl(questionId);

            return this.cacheOfQuestionsWhichCustomValidationDependsOnQuestion[questionId];
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

        public IEnumerable<QuestionIdAndVariableName> GetQuestionsInvolvedInCustomEnablementConditionOfGroup(Guid groupId)
        {
            if (!this.cacheOfQuestionsInvolvedInCustomEnablementConditionOfGroup.ContainsKey(groupId))
                this.cacheOfQuestionsInvolvedInCustomEnablementConditionOfGroup[groupId]
                    = this.GetQuestionsInvolvedInCustomEnablementConditionOfGroupImpl(groupId);

            return this.cacheOfQuestionsInvolvedInCustomEnablementConditionOfGroup[groupId];
        }

        public IEnumerable<QuestionIdAndVariableName> GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(Guid questionId)
        {
            if (!this.cacheOfQuestionsInvolvedInCustomEnablementConditionOfQuestion.ContainsKey(questionId))
                this.cacheOfQuestionsInvolvedInCustomEnablementConditionOfQuestion[questionId]
                    = this.GetQuestionsInvolvedInCustomEnablementConditionOfQuestionImpl(questionId);

            return this.cacheOfQuestionsInvolvedInCustomEnablementConditionOfQuestion[questionId];
        }

        public IEnumerable<Guid> GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId)
        {
            if (!this.cacheOfGroupsWhichCustomEnablementConditionDependsOnQuestion.ContainsKey(questionId))
                this.cacheOfGroupsWhichCustomEnablementConditionDependsOnQuestion[questionId]
                    = this.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestionImpl(questionId);

            return this.cacheOfGroupsWhichCustomEnablementConditionDependsOnQuestion[questionId];
        }

        public IEnumerable<Guid> GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId)
        {
            if (!this.cacheOfQuestionsWhichCustomEnablementConditionDependsOnQuestion.ContainsKey(questionId))
                this.cacheOfQuestionsWhichCustomEnablementConditionDependsOnQuestion[questionId]
                    = this.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestionImpl(questionId);

            return this.cacheOfQuestionsWhichCustomEnablementConditionDependsOnQuestion[questionId];
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
            var autoPropagatingQuestion = (IAutoPropagateQuestion) question;
            if (autoPropagatingQuestion != null)
            {
                foreach (Guid groupId in autoPropagatingQuestion.Triggers)
                {
                    this.ThrowIfGroupDoesNotExist(groupId);
                }

                return autoPropagatingQuestion.Triggers.ToList();
            }
            
            //### roster
            return this.GetAllGroups().Where(x => x.RosterSizeQuestionId == questionId).Select(x => x.PublicKey);
        }

        public int? GetMaxValueForNumericQuestion(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            this.ThrowIfQuestionDoesNotSupportRoster(question.PublicKey);

            //### old questionnaires supporting
            var autoPropagatingQuestion = (IAutoPropagateQuestion)question;
            if (autoPropagatingQuestion != null)
                return autoPropagatingQuestion.MaxValue;

            //### roster
            var numericQuestion = (INumericQuestion) question;
            return numericQuestion.MaxValue;
        }

        public IEnumerable<Guid> GetParentRosterGroupsForQuestionStartingFromTop(Guid questionId)
        {
            return this
                .GetAllParentGroupsForQuestionStartingFromBottom(questionId)
                .Where(this.IsRosterGroup)
                .Reverse()
                .ToList();
        }

        public IEnumerable<Guid> GetParentRosterGroupsAndGroupItselfIfRosterStartingFromTop(Guid groupId)
        {
            IGroup group = this.GetGroupOrThrow(groupId);

            return this
                .GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(@group)
                .Where(this.IsRosterGroup)
                .Reverse()
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

        public IEnumerable<Guid> GetAllQuestionsWithNotEmptyCustomEnablementConditions()
        {
            return
               from question in this.GetAllQuestions()
               where IsExpressionDefined(question.ConditionExpression)
               select question.PublicKey;
        }

        public IEnumerable<Guid> GetAllGroupsWithNotEmptyCustomEnablementConditions()
        {
            return
                from @group in this.GetAllGroups()
                where IsExpressionDefined(@group.ConditionExpression)
                select @group.PublicKey;
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

        public IEnumerable<Guid> GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(Guid groupId)
        {
            if (!this.cacheOfGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions.ContainsKey(groupId))
                this.cacheOfGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions[groupId]
                    = this.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditionsImpl(groupId);

            return this.cacheOfGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions[groupId];
        }

        public IEnumerable<Guid> GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditions(Guid groupId)
        {
            if (!this.cacheOfUnderlyingQuestionsWithNotEmptyCustomEnablementConditions.ContainsKey(groupId))
                this.cacheOfUnderlyingQuestionsWithNotEmptyCustomEnablementConditions[groupId] = this.GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditionsImpl(groupId);

            return this.cacheOfUnderlyingQuestionsWithNotEmptyCustomEnablementConditions[groupId];
        }

        public IEnumerable<Guid> GetUnderlyingQuestionsWithNotEmptyCustomValidationExpressions(Guid groupId)
        {
            if (!this.cacheOfUnderlyingQuestionsWithNotEmptyCustomValidationExpressions.ContainsKey(groupId))
                this.cacheOfUnderlyingQuestionsWithNotEmptyCustomValidationExpressions[groupId] = this.GetUnderlyingQuestionsWithNotEmptyCustomValidationExpressionsImpl(groupId);

            return this.cacheOfUnderlyingQuestionsWithNotEmptyCustomValidationExpressions[groupId];
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

        public IEnumerable<Guid> GetUnderlyingMandatoryQuestions(Guid groupId)
        {
            if (!this.cacheOfUnderlyingMandatoryQuestions.ContainsKey(groupId))
                this.cacheOfUnderlyingMandatoryQuestions[groupId] = this.GetUnderlyingMandatoryQuestionsImpl(groupId);

            return this.cacheOfUnderlyingMandatoryQuestions[groupId];
        }


        private static QuestionnaireDocument CastToQuestionnaireDocumentOrThrow(IQuestionnaireDocument source)
        {
            var document = source as QuestionnaireDocument;

            if (document == null)
                throw new QuestionnaireException(string.Format("Cannot import questionnaire with a document of a not supported type {0}.", source.GetType()));

            return document;
        }

        private IEnumerable<QuestionIdAndVariableName> GetQuestionsInvolvedInCustomValidationImpl(Guid questionId)
        {
            string validationExpression = this.GetCustomValidationExpression(questionId);

            return GetQuestionsInvolvedInCustomValidation(questionId, validationExpression);
        }

        private IEnumerable<QuestionIdAndVariableName> GetQuestionsInvolvedInCustomValidation(Guid questionId, string validationExpression)
        {
            if (!IsExpressionDefined(validationExpression))
                return Enumerable.Empty<QuestionIdAndVariableName>();

            IEnumerable<string> identifiersUsedInExpression = ExpressionProcessor.GetIdentifiersUsedInExpression(validationExpression);

            return DistinctlyResolveExpressionIdentifiersToExistingQuestionIdsReplacingThisIdentifierOrThrow(
                identifiersUsedInExpression, questionId, validationExpression);
        }

        private IEnumerable<Guid> GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestionImpl(Guid questionId)
        {
            this.ThrowIfQuestionDoesNotExist(questionId);

            return Enumerable.ToList(
                from question in this.GetAllQuestions()
                where  this.DoesQuestionCustomValidationDependOnSpecifiedQuestion(question.PublicKey, specifiedQuestionId: questionId)
                       && questionId != question.PublicKey
                select question.PublicKey
            );
        }

        private IEnumerable<QuestionIdAndVariableName> GetQuestionsInvolvedInCustomEnablementConditionOfGroupImpl(Guid groupId)
        {
            string enablementCondition = this.GetCustomEnablementConditionForGroup(groupId);

            return this.GetQuestionsInvolvedInCustomEnablementCondition(enablementCondition);
        }

        private IEnumerable<QuestionIdAndVariableName> GetQuestionsInvolvedInCustomEnablementConditionOfQuestionImpl(Guid questionId)
        {
            string enablementCondition = this.GetCustomEnablementConditionForQuestion(questionId);

            return this.GetQuestionsInvolvedInCustomEnablementCondition(enablementCondition);
        }

        private IEnumerable<Guid> GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestionImpl(Guid questionId)
        {
            this.ThrowIfQuestionDoesNotExist(questionId);

            return Enumerable.ToList(
                from @group in this.GetAllGroups()
                let enablementCondition = this.GetCustomEnablementConditionForGroup(@group.PublicKey)
                where this.DoesCustomEnablementConditionDependOnSpecifiedQuestion(enablementCondition, questionId)
                select @group.PublicKey
            );
        }

        private IEnumerable<Guid> GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestionImpl(Guid questionId)
        {
            this.ThrowIfQuestionDoesNotExist(questionId);

            return Enumerable.ToList(
                from question in this.GetAllQuestions()
                let enablementCondition = this.GetCustomEnablementConditionForQuestion(question.PublicKey)
                where this.DoesCustomEnablementConditionDependOnSpecifiedQuestion(enablementCondition, questionId)
                select question.PublicKey
            );
        }

        private IEnumerable<Guid> GetAllUnderlyingQuestionsImpl(Guid groupId)
        {
            return this
                .GetGroupOrThrow(groupId)
                .Find<IQuestion>(_ => true)
                .Select(question => question.PublicKey)
                .ToList();
        }

        private IEnumerable<Guid> GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditionsImpl(Guid groupId)
        {
            IGroup group = this.GetGroupOrThrow(groupId);

            List<Guid> groupsWithNotEmptyCustomEnablementConditions
                = group
                .Find<IGroup>(g => IsExpressionDefined(g.ConditionExpression))
                .Select(g => g.PublicKey)
                .ToList();

            if (IsExpressionDefined(group.ConditionExpression))
            {
                groupsWithNotEmptyCustomEnablementConditions.Add(group.PublicKey);
            }

            return groupsWithNotEmptyCustomEnablementConditions;
        }

        private IEnumerable<Guid> GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditionsImpl(Guid groupId)
        {
            return this
                .GetGroupOrThrow(groupId)
                .Find<IQuestion>(question => IsExpressionDefined(question.ConditionExpression))
                .Select(question => question.PublicKey)
                .ToList();
        }

        private IEnumerable<Guid> GetUnderlyingQuestionsWithNotEmptyCustomValidationExpressionsImpl(Guid groupId)
        {
            return this
                .GetGroupOrThrow(groupId)
                .Find<IQuestion>(question => IsExpressionDefined(question.ValidationExpression))
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

        private int GetRosterLevelForQuestion(Guid questionId, Func<Guid, IEnumerable<Guid>> getAllParentGroupsForQuestion, Func<Guid, bool> isRosterGroup)
        {
            return getAllParentGroupsForQuestion(questionId).Count(isRosterGroup);
        }

        private IEnumerable<Guid> GetAllParentGroupsForQuestionStartingFromBottom(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            var parentGroup = (IGroup) question.GetParent();

            return this.GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(parentGroup);
        }

        private IEnumerable<Guid> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(IGroup group)
        {
            return GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(group, this.innerDocument).Select(_ => _.PublicKey);
        }

        private IEnumerable<IGroup> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(IGroup group, QuestionnaireDocument document)
        {
            var parentGroups = new List<IGroup>();

            while (group != document)
            {
                parentGroups.Add(group);
                group = (IGroup) group.GetParent();
            }

            return parentGroups;
        }

        private IEnumerable<QuestionIdAndVariableName> GetQuestionsInvolvedInCustomEnablementCondition(string enablementCondition)
        {
            if (!IsExpressionDefined(enablementCondition))
                return Enumerable.Empty<QuestionIdAndVariableName>();

            IEnumerable<string> identifiersUsedInExpression = ExpressionProcessor.GetIdentifiersUsedInExpression(enablementCondition);

            return identifiersUsedInExpression
                .Select(identifier => this.ParseExpressionIdentifierToExistingQuestionIdIgnoringThisIdentifierOrThrow(identifier, enablementCondition))
                .ToList();
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

        private IEnumerable<QuestionIdAndVariableName> DistinctlyResolveExpressionIdentifiersToExistingQuestionIdsReplacingThisIdentifierOrThrow(
            IEnumerable<string> identifiers, Guid contextQuestionId, string expression)
        {
            var distinctQuestionIds = new HashSet<QuestionIdAndVariableName>();

            foreach (var identifier in identifiers)
            {
                if (IsSpecialThisIdentifier(identifier))
                {
                    var contextQuestion = this.GetQuestion(contextQuestionId);
                    distinctQuestionIds.Add(new QuestionIdAndVariableName(contextQuestion.PublicKey, contextQuestion.StataExportCaption));
                }
                else
                {
                    distinctQuestionIds.Add(ParseExpressionIdentifierToExistingQuestionIdIgnoringThisIdentifierOrThrow(identifier,
                        expression));
                }
            }

            return distinctQuestionIds;
        }

        private IQuestion GetQuestionByStringIdOrVariableName(string identifier)
        {
            Guid parsedId;
            return !Guid.TryParse(identifier, out parsedId) ? this.GetQuestionByStataCaption(identifier) : this.GetQuestion(parsedId);
        }

        private QuestionIdAndVariableName ParseExpressionIdentifierToExistingQuestionIdIgnoringThisIdentifierOrThrow(string identifier,
            string expression)
        {
            IQuestion question = GetQuestionByStringIdOrVariableName(identifier);

            if (question == null)
                throw new QuestionnaireException(string.Format(
                    "Identifier '{0}' from expression '{1}' is not valid question identifier. Question with such a identifier is missing.",
                    identifier, expression));

            return new QuestionIdAndVariableName(question.PublicKey, question.StataExportCaption);
        }

        private void ThrowIfVerifierFindsErrors(QuestionnaireDocument document)
        {
            var errors = QuestionnaireVerifier.Verify(document);
            if (errors.Any())
                throw new QuestionnaireVerificationException(string.Format("Questionnaire '{0}' can't be imported", document.Title),
                    errors.ToArray());
        }

        private static bool IsSpecialThisIdentifier(string identifier)
        {
            return identifier.ToLower() == "this";
        }

        private static bool IsExpressionDefined(string expression)
        {
            return !string.IsNullOrWhiteSpace(expression);
        }

        private bool DoesQuestionCustomValidationDependOnSpecifiedQuestion(Guid questionId, Guid specifiedQuestionId)
        {
            IEnumerable<QuestionIdAndVariableName> involvedQuestions = this.GetQuestionsInvolvedInCustomValidation(questionId);

            bool isSpecifiedQuestionInvolved = involvedQuestions.Any(question => question.Id == specifiedQuestionId);

            return isSpecifiedQuestionInvolved;
        }

        private bool DoesCustomEnablementConditionDependOnSpecifiedQuestion(string enablementCondition, Guid specifiedQuestionId)
        {
            IEnumerable<QuestionIdAndVariableName> involvedQuestions = this.GetQuestionsInvolvedInCustomEnablementCondition(enablementCondition);

            bool isSpecifiedQuestionInvolved = involvedQuestions.Any(question => question.Id == specifiedQuestionId);

            return isSpecifiedQuestionInvolved;
        }

        private bool DoesQuestionSupportRoster(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            return DoesQuestionSupportRoster(question);
        }

        private static bool DoesQuestionSupportRoster(IQuestion question)
        {
                   //### roster
            return question.QuestionType == QuestionType.Numeric ||
                   //### old questionnaires supporting
                  (question.QuestionType == QuestionType.AutoPropagate && question is IAutoPropagateQuestion);

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

        private void ThrowIfGroupDoesNotExist(Guid groupId)
        {
            this.GetGroupOrThrow(groupId);
        }

        private IGroup GetGroupOrThrow(Guid groupId)
        {
            IGroup group = this.GetGroup(groupId);

            if (group == null)
                throw new QuestionnaireException(string.Format("Group with id '{0}' is not found.", groupId));

            return group;
        }

        private IGroup GetGroup(Guid groupId)
        {
            return this.GroupCache.ContainsKey(groupId)
                ? this.GroupCache[groupId]
                : null;
        }

        private void ThrowIfQuestionDoesNotExist(Guid questionId)
        {
            this.GetQuestionOrThrow(questionId);
        }

        private IQuestion GetQuestionOrThrow(Guid questionId)
        {
            IQuestion question = this.GetQuestion(questionId);

            if (question == null)
                throw new QuestionnaireException(string.Format("Question with id '{0}' is not found.", questionId));

            return question;
        }

        private IQuestion GetQuestion(Guid questionId)
        {
            return this.QuestionCache.ContainsKey(questionId)
                ? this.QuestionCache[questionId]
                : null;
        }
        
        private static string FormatQuestionForException(IQuestion question)
        {
            return string.Format("'{0} [{1}] ({2:N})'",
                question.QuestionText ?? "<<NO QUESTION TITLE>>",
                question.StataExportCaption ?? "<<NO VARIABLE NAME>>",
                question.PublicKey);
        }
    }
}