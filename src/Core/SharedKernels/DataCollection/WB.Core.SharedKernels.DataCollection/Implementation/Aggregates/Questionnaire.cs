using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Domain;
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
using WB.Core.SharedKernels.DataCollection.Implementation.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class Questionnaire : AggregateRootMappedByConvention, IQuestionnaire, ISnapshotable<QuestionnaireState>
    {
        #region State

        private QuestionnaireDocument innerDocument = new QuestionnaireDocument();
        private Dictionary<Guid, IQuestion> questionCache = null;
        private Dictionary<Guid, IGroup> groupCache = null;
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfQuestionsInvolvedInCustomValidationOfQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfQuestionsWhichCustomValidationDependsOnQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfQuestionsInvolvedInCustomEnablementConditionOfGroup = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfQuestionsInvolvedInCustomEnablementConditionOfQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfGroupsWhichCustomEnablementConditionDependsOnQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfQuestionsWhichCustomEnablementConditionDependsOnQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingQuestions = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions = new Dictionary<Guid, IEnumerable<Guid>>();
        private Dictionary<Guid, IEnumerable<Guid>> cacheOfUnderlyingQuestionsWithNotEmptyCustomEnablementConditions = new Dictionary<Guid, IEnumerable<Guid>>();

        protected internal void OnTemplateImported(TemplateImported e)
        {
            this.innerDocument = e.Source;
            this.innerDocument.ConnectChildsWithParent();

            this.questionCache = null;
            this.groupCache = null;
            this.cacheOfQuestionsInvolvedInCustomValidationOfQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfQuestionsWhichCustomValidationDependsOnQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfQuestionsInvolvedInCustomEnablementConditionOfGroup = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfQuestionsInvolvedInCustomEnablementConditionOfQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfGroupsWhichCustomEnablementConditionDependsOnQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfQuestionsWhichCustomEnablementConditionDependsOnQuestion = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfUnderlyingQuestions = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions = new Dictionary<Guid, IEnumerable<Guid>>();
            this.cacheOfUnderlyingQuestionsWithNotEmptyCustomEnablementConditions = new Dictionary<Guid, IEnumerable<Guid>>();
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
        private IExpressionProcessor ExpressionProcessor
        {
            get { return ServiceLocator.Current.GetInstance<IExpressionProcessor>(); }
        }

        #endregion


        public Questionnaire(){}

        public Questionnaire(Guid createdBy, IQuestionnaireDocument source)
            : base(source.PublicKey)
        {
            this.ImportQuestionnaire(createdBy, source);
        }


        public void ImportQuestionnaire(Guid createdBy, IQuestionnaireDocument source)
        {
            QuestionnaireDocument document = CastToQuestionnaireDocumentOrThrow(source);
            ThrowIfSomePropagatingQuestionsHaveNoAssociatedGroups(document);
            ThrowIfSomePropagatedGroupsHaveNoPropagatingQuestionsPointingToThem(document);
            ThrowIfSomePropagatedGroupsHaveMoreThanOnePropagatingQuestionPointingToThem(document);

            document.CreatedBy = this.innerDocument.CreatedBy;

            this.ApplyEvent(new TemplateImported() {Source = document});
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
                    "Cannot return answer options for queston with id '{0}' because it's type {1} does not support answer options.",
                    questionId, question.QuestionType));

            return question.Answers.Select(answer => this.ParseAnswerOptionValueOrThrow(answer.AnswerValue, questionId)).ToList();
        }

        public bool IsCustomValidationDefined(Guid questionId)
        {
            var validationExpression = this.GetCustomValidationExpression(questionId);

            return IsExpressionDefined(validationExpression);
        }

        public IEnumerable<Guid> GetQuestionsInvolvedInCustomValidation(Guid questionId)
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

        public IEnumerable<Guid> GetQuestionsWithInvalidCustomValidationExpressions()
        {
            var invalidQuestions = new List<Guid>();

            foreach (IQuestion question in this.GetAllQuestions())
            {
                try
                {
                    this.GetQuestionsInvolvedInCustomValidation(question.PublicKey);
                }
                catch (Exception exception)
                {
                    invalidQuestions.Add(question.PublicKey);

                    this.Logger.Info(
                        string.Format(
                            "Validation expression '{0}' for question '{1}' treated invalid " +
                            "because exception occurred when tried to determine questions involved in validation expression.",
                            question.ValidationExpression, question.PublicKey),
                        exception);
                }
            }

            return invalidQuestions;
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

        public IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionOfGroup(Guid groupId)
        {
            if (!this.cacheOfQuestionsInvolvedInCustomEnablementConditionOfGroup.ContainsKey(groupId))
                this.cacheOfQuestionsInvolvedInCustomEnablementConditionOfGroup[groupId]
                    = this.GetQuestionsInvolvedInCustomEnablementConditionOfGroupImpl(groupId);

            return this.cacheOfQuestionsInvolvedInCustomEnablementConditionOfGroup[groupId];
        }

        public IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(Guid questionId)
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

        public IEnumerable<Guid> GetGroupsWithInvalidCustomEnablementConditions()
        {
            var invalidGroups = new List<Guid>();

            foreach (IGroup @group in this.GetAllGroups())
            {
                try
                {
                    this.GetQuestionsInvolvedInCustomEnablementConditionOfGroup(@group.PublicKey);
                }
                catch (Exception exception)
                {
                    invalidGroups.Add(@group.PublicKey);

                    this.Logger.Info(
                        string.Format(
                            "Enablement condition '{0}' for group '{1}' treated invalid " +
                            "because exception occurred when tried to determine questions involved in enablement condition.",
                            @group.ConditionExpression, @group.PublicKey),
                        exception);
                }
            }

            return invalidGroups;
        }

        public IEnumerable<Guid> GetQuestionsWithInvalidCustomEnablementConditions()
        {
            var invalidQuestions = new List<Guid>();

            foreach (IQuestion question in this.GetAllQuestions())
            {
                try
                {
                    this.GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(question.PublicKey);
                }
                catch (Exception exception)
                {
                    invalidQuestions.Add(question.PublicKey);

                    this.Logger.Info(
                        string.Format(
                            "Enablement condition '{0}' for question '{1}' treated invalid " +
                            "because exception occurred when tried to determine questions involved in enablement condition.",
                            question.ConditionExpression, question.PublicKey),
                        exception);
                }
            }

            return invalidQuestions;
        }

        public bool ShouldQuestionPropagateGroups(Guid questionId)
        {
            return this.DoesQuestionSupportPropagation(questionId)
                && this.GetGroupsPropagatedByQuestion(questionId).Any();
        }

        public IEnumerable<Guid> GetGroupsPropagatedByQuestion(Guid questionId)
        {
            if (!this.DoesQuestionSupportPropagation(questionId))
                return Enumerable.Empty<Guid>();

            IQuestion question = this.GetQuestionOrThrow(questionId);
            var autoPropagatingQuestion = (IAutoPropagateQuestion) question;

            foreach (Guid groupId in autoPropagatingQuestion.Triggers)
            {
                this.ThrowIfGroupDoesNotExist(groupId);
            }

            return autoPropagatingQuestion.Triggers.ToList();
        }

        public int GetMaxAnswerValueForPropagatingQuestion(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            this.ThrowIfQuestionDoesNotSupportPropagation(question.PublicKey);
            var autoPropagatingQuestion = (IAutoPropagateQuestion) question;

            return autoPropagatingQuestion.MaxValue;
        }

        public IEnumerable<Guid> GetPropagatingQuestionsWhichReferToMissingOrNotPropagatableGroups()
        {
            return (
                from question in this.GetAllPropagatingQuestions()
                let questionHasMissingGroup = question.Triggers.Any(groupId => !this.HasGroup(groupId) || !this.IsGroupPropagatable(groupId))
                where questionHasMissingGroup
                select question.PublicKey
            ).ToList();
        }

        public IEnumerable<Guid> GetParentPropagatableGroupsForQuestionStartingFromTop(Guid questionId)
        {
            return this
                .GetAllParentGroupsForQuestionStartingFromBottom(questionId)
                .Where(this.IsGroupPropagatable)
                .Reverse()
                .ToList();
        }

        public IEnumerable<Guid> GetParentPropagatableGroupsAndGroupItselfIfPropagatableStartingFromTop(Guid groupId)
        {
            IGroup group = this.GetGroupOrThrow(groupId);

            return this
                .GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(@group)
                .Where(this.IsGroupPropagatable)
                .Reverse()
                .ToList();
        }

        public int GetPropagationLevelForQuestion(Guid questionId)
        {
            this.ThrowIfQuestionDoesNotExist(questionId);

            return this
                .GetAllParentGroupsForQuestion(questionId)
                .Count(this.IsGroupPropagatable);
        }

        public int GetPropagationLevelForGroup(Guid groupId)
        {
            IGroup group = this.GetGroupOrThrow(groupId);

            return this
                .GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(group)
                .Count(this.IsGroupPropagatable);
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

        public bool IsGroupPropagatable(Guid groupId)
        {
            IGroup @group = this.GetGroupOrThrow(groupId);

            return @group.Propagated == Propagate.AutoPropagated;
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


        private static QuestionnaireDocument CastToQuestionnaireDocumentOrThrow(IQuestionnaireDocument source)
        {
            var document = source as QuestionnaireDocument;

            if (document == null)
                throw new QuestionnaireException(string.Format("Cannot import questionnaire with a document of a not supported type {0}.", source.GetType()));

            return document;
        }

        private static void ThrowIfSomePropagatingQuestionsHaveNoAssociatedGroups(QuestionnaireDocument document)
        {
            IEnumerable<IAutoPropagateQuestion> propagatingQuestionsWithNoAssociatedGroups
                = document.Find<IAutoPropagateQuestion>(question => question.Triggers.Count == 0);

            if (propagatingQuestionsWithNoAssociatedGroups.Any())
                throw new QuestionnaireException(string.Format(
                    "Following questions are propagating and are expected to have associated groups, but they have no groups associated:{0}{1}",
                    Environment.NewLine,
                    string.Join(Environment.NewLine, propagatingQuestionsWithNoAssociatedGroups.Select(FormatQuestionForException))));
        }

        private static void ThrowIfSomePropagatedGroupsHaveNoPropagatingQuestionsPointingToThem(QuestionnaireDocument document)
        {
            IEnumerable<IGroup> propagatedGroupsWithNoPropagatingQuestionsPointingToThem = document.Find<IGroup>(group
                => group.Propagated == Propagate.AutoPropagated
                && GetPropagatingQuestionsPointingToPropagatedGroup(group, document).Count() == 0);

            if (propagatedGroupsWithNoPropagatingQuestionsPointingToThem.Any())
                throw new QuestionnaireException(string.Format(
                    "Following groups are propagated but there are no propagating questions which point to these groups:{0}{1}",
                    Environment.NewLine,
                    string.Join(Environment.NewLine, propagatedGroupsWithNoPropagatingQuestionsPointingToThem.Select(FormatGroupForException))));
        }

        private void ThrowIfSomePropagatedGroupsHaveMoreThanOnePropagatingQuestionPointingToThem(QuestionnaireDocument document)
        {
            IEnumerable<IGroup> propagatedGroupsWithMoreThanOnePropagatingQuestionPointingToThem = document.Find<IGroup>(group
                => group.Propagated == Propagate.AutoPropagated
                && GetPropagatingQuestionsPointingToPropagatedGroup(group, document).Count() > 1);

            if (propagatedGroupsWithMoreThanOnePropagatingQuestionPointingToThem.Any())
                throw new QuestionnaireException(string.Format(
                    "Following groups are propagated but there is more than one propagating question which points to these groups:{0}{1}",
                    Environment.NewLine,
                    string.Join(Environment.NewLine, propagatedGroupsWithMoreThanOnePropagatingQuestionPointingToThem.Select(FormatGroupForException))));
        }


        private static IEnumerable<IQuestion> GetPropagatingQuestionsPointingToPropagatedGroup(IGroup group, QuestionnaireDocument document)
        {
            return document.Find<IAutoPropagateQuestion>(question => question.Triggers.Contains(group.PublicKey));
        }


        private IEnumerable<Guid> GetQuestionsInvolvedInCustomValidationImpl(Guid questionId)
        {
            string validationExpression = this.GetCustomValidationExpression(questionId);

            if (!IsExpressionDefined(validationExpression))
                return Enumerable.Empty<Guid>();

            IEnumerable<string> identifiersUsedInExpression = this.ExpressionProcessor.GetIdentifiersUsedInExpression(validationExpression);

            return this.DistinctlyResolveExpressionIdentifiersToExistingQuestionIdsReplacingThisIdentifierOrThrow(
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

        private IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionOfGroupImpl(Guid groupId)
        {
            string enablementCondition = this.GetCustomEnablementConditionForGroup(groupId);

            return this.GetQuestionsInvolvedInCustomEnablementCondition(enablementCondition);
        }

        private IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionOfQuestionImpl(Guid questionId)
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


        private IEnumerable<IGroup> GetAllGroups()
        {
            return this.GroupCache.Values;
        }

        private IEnumerable<IQuestion> GetAllQuestions()
        {
            return this.QuestionCache.Values;
        }

        private IEnumerable<IAutoPropagateQuestion> GetAllPropagatingQuestions()
        {
            return this
                .GetAllQuestions()
                .Where(DoesQuestionSupportPropagation)
                .Cast<IAutoPropagateQuestion>();
        }

        private IEnumerable<Guid> GetAllParentGroupsForQuestionStartingFromBottom(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            var parentGroup = (IGroup) question.GetParent();

            return this.GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(parentGroup);
        }

        private IEnumerable<Guid> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(IGroup group)
        {
            var parentGroups = new List<Guid>();

            while (group != this.innerDocument)
            {
                parentGroups.Add(group.PublicKey);
                group = (IGroup) group.GetParent();
            }

            return parentGroups;
        }

        private IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementCondition(string enablementCondition)
        {
            if (!IsExpressionDefined(enablementCondition))
                return Enumerable.Empty<Guid>();

            IEnumerable<string> identifiersUsedInExpression = this.ExpressionProcessor.GetIdentifiersUsedInExpression(enablementCondition);

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

        private IEnumerable<Guid> DistinctlyResolveExpressionIdentifiersToExistingQuestionIdsReplacingThisIdentifierOrThrow(
            IEnumerable<string> identifiers, Guid contextQuestionId, string expression)
        {
            var distinctQuestionIds = new HashSet<Guid>();

            foreach (var identifier in identifiers)
            {
                if (IsSpecialThisIdentifier(identifier))
                {
                    if (!distinctQuestionIds.Contains(contextQuestionId))
                    {
                        distinctQuestionIds.Add(contextQuestionId);
                    }

                    continue;
                }

                Guid parsedId;
                if (!Guid.TryParse(identifier, out parsedId))
                    throw new QuestionnaireException(string.Format(
                        "Identifier '{0}' from expression '{1}' is not a 'this' keyword nor a valid guid.",
                        identifier, expression));
                this.ThrowIfThereAreNoCorrespondingQuestionsForExpressionIdentifierParsedToGuid(identifier, expression,
                                                                                                parsedId);
                distinctQuestionIds.Add(parsedId);
            }

            return distinctQuestionIds;
        }

        private Guid ParseExpressionIdentifierToExistingQuestionIdIgnoringThisIdentifierOrThrow(string identifier, string expression)
        {
            Guid parsedId;
            if (!Guid.TryParse(identifier, out parsedId))
                throw new QuestionnaireException(string.Format(
                    "Identifier '{0}' from expression '{1}' is not a valid guid.",
                    identifier, expression));

            this.ThrowIfThereAreNoCorrespondingQuestionsForExpressionIdentifierParsedToGuid(identifier, expression, parsedId);

            return parsedId;
        }

        private void ThrowIfThereAreNoCorrespondingQuestionsForExpressionIdentifierParsedToGuid(string identifier, string expression, Guid parsedId)
        {
            if (!this.HasQuestion(parsedId))
                throw new QuestionnaireException(string.Format(
                    "Identifier '{0}' from expression '{1}' is a valid guid '{2}' but questionnaire has no questions with such id.",
                    identifier, expression, parsedId));
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
            IEnumerable<Guid> involvedQuestions = this.GetQuestionsInvolvedInCustomValidation(questionId);

            bool isSpecifiedQuestionInvolved = involvedQuestions.Contains(specifiedQuestionId);

            return isSpecifiedQuestionInvolved;
        }

        private bool DoesCustomEnablementConditionDependOnSpecifiedQuestion(string enablementCondition, Guid specifiedQuestionId)
        {
            IEnumerable<Guid> involvedQuestions = this.GetQuestionsInvolvedInCustomEnablementCondition(enablementCondition);

            bool isSpecifiedQuestionInvolved = involvedQuestions.Contains(specifiedQuestionId);

            return isSpecifiedQuestionInvolved;
        }

        private bool DoesQuestionSupportPropagation(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            return DoesQuestionSupportPropagation(question);
        }

        private static bool DoesQuestionSupportPropagation(IQuestion question)
        {
            return (question.QuestionType == QuestionType.Numeric || question.QuestionType == QuestionType.AutoPropagate)
                && (question is IAutoPropagateQuestion);
        }

        private void ThrowIfQuestionDoesNotSupportPropagation(Guid questionId)
        {
            if (!this.DoesQuestionSupportPropagation(questionId))
                throw new QuestionnaireException(string.Format("Question with id '{0}' is not a propagating question.", questionId));
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

        private static string FormatGroupForException(IGroup group)
        {
            return string.Format("'{0} ({1:N})'",
                group.Title ?? "<<NO GROUP TITLE>>",
                group.PublicKey);
        }
    }
}