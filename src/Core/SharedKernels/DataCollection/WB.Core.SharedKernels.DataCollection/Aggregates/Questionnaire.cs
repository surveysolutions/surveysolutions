using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.AbstractFactories;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Entities.Composite;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Main.Core.Utility;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;

namespace WB.Core.SharedKernels.DataCollection.Aggregates
{
    public class Questionnaire : AggregateRootMappedByConvention, IQuestionnaire
    {
        #region State

        private QuestionnaireDocument innerDocument = new QuestionnaireDocument();
        private Dictionary<Guid, IQuestion> questionCache = null;
        private Dictionary<Guid, IGroup> groupCache = null;

        protected internal void OnTemplateImported(TemplateImported e)
        {
            this.innerDocument = e.Source;
            this.questionCache = null;
            this.groupCache = null;
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
            ImportQuestionnaire(createdBy, source);
        }


        public void ImportQuestionnaire(Guid createdBy, IQuestionnaireDocument source)
        {
           
            var document = source as QuestionnaireDocument;
            if (document == null)
                throw new DomainException(DomainExceptionType.TemplateIsInvalid
                                          , "only QuestionnaireDocuments are supported for now");
            document.CreatedBy = this.innerDocument.CreatedBy;
            ApplyEvent(new TemplateImported() {Source = document});
           
        }

        public void CreateInterviewWithFeaturedQuestions(Guid interviewId, UserLight creator, UserLight responsible, List<QuestionAnswer> featuredAnswers)
        #warning probably a factory should be used here
        {
            // TODO: check is it good to create new AR form another?
            new CompleteQuestionnaireAR(interviewId, this.innerDocument, creator, responsible, featuredAnswers);
        }

        public void CreateCompletedQ(Guid completeQuestionnaireId, UserLight creator)
        #warning probably a factory should be used here
        {
            // TODO: check is it good to create new AR form another?
            // Do we need Saga here?
            new CompleteQuestionnaireAR(completeQuestionnaireId, this.innerDocument, creator);
        }


        public IQuestion GetQuestionByStataCaption(string stataCaption)
        {
            return this.innerDocument.FirstOrDefault<IQuestion>(q => q.StataExportCaption == stataCaption);
        }

        public bool HasQuestion(Guid questionId)
        {
            return this.GetQuestion(questionId) != null;
        }

        public QuestionType GetQuestionType(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            return question.QuestionType;
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
            string validationExpression = this.GetCustomValidationExpression(questionId);

            if (!IsExpressionDefined(validationExpression))
                return Enumerable.Empty<Guid>();

            IEnumerable<string> identifiersUsedInExpression = this.ExpressionProcessor.GetIdentifiersUsedInExpression(validationExpression);

            return identifiersUsedInExpression
                .Select(identifier => this.ParseExpressionIdentifierToExistingQuestionIdResolvingThisIdentifierOrThrow(identifier, questionId, validationExpression))
                .ToList();
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
            this.ThrowIfQuestionDoesNotExist(questionId);

            return Enumerable.ToList(
                from question in this.GetAllQuestions()
                where this.DoesQuestionCustomValidationDependOnSpecifiedQuestion(question.PublicKey, specifiedQuestionId: questionId)
                select question.PublicKey
            );
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

        public IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionForGroup(Guid groupId)
        {
            string enablementCondition = this.GetCustomEnablementConditionForGroup(groupId);

            return this.GetQuestionsInvolvedInCustomEnablementCondition(enablementCondition);
        }

        public IEnumerable<Guid> GetQuestionsInvolvedInCustomEnablementConditionForQuestion(Guid questionId)
        {
            string enablementCondition = this.GetCustomEnablementConditionForQuestion(questionId);

            return this.GetQuestionsInvolvedInCustomEnablementCondition(enablementCondition);
        }

        public IEnumerable<Guid> GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId)
        {
            this.ThrowIfQuestionDoesNotExist(questionId);

            return Enumerable.ToList(
                from @group in this.GetAllGroups()
                let enablementCondition = this.GetCustomEnablementConditionForGroup(@group.PublicKey)
                where this.DoesCustomEnablementConditionDependOnSpecifiedQuestion(enablementCondition, questionId)
                select @group.PublicKey
            );
        }

        public IEnumerable<Guid> GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId)
        {
            this.ThrowIfQuestionDoesNotExist(questionId);

            return Enumerable.ToList(
                from question in this.GetAllQuestions()
                let enablementCondition = this.GetCustomEnablementConditionForQuestion(question.PublicKey)
                where this.DoesCustomEnablementConditionDependOnSpecifiedQuestion(enablementCondition, questionId)
                select question.PublicKey
            );
        }

        public IEnumerable<Guid> GetGroupsWithInvalidCustomEnablementConditions()
        {
            var invalidGroups = new List<Guid>();

            foreach (IGroup @group in this.GetAllGroups())
            {
                try
                {
                    this.GetQuestionsInvolvedInCustomEnablementConditionForGroup(@group.PublicKey);
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
                    this.GetQuestionsInvolvedInCustomEnablementConditionForQuestion(question.PublicKey);
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

        public IEnumerable<Guid> GetPropagatingQuestionsWhichReferToMissingGroups()
        {
            return (
                from question in this.GetAllPropagatingQuestions()
                let questionHasMissingGroup = question.Triggers.Any(groupId => !this.HasGroup(groupId))
                where questionHasMissingGroup
                select question.PublicKey
            ).ToList();
        }

        public IEnumerable<Guid> GetParentPropagatableGroupsForQuestionStartingFromTop(Guid questionId)
        {
            return this
                .GetAllParentGroupsForQuestionStartingFromBottom(questionId)
                .Where(this.IsPropogatableGroup)
                .Reverse()
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

            this.innerDocument.ConnectChildsWithParent();

            var parentGroups = new List<Guid>();

            IComposite parent = question.GetParent();
            while (parent != this.innerDocument)
            {
                parentGroups.Add(parent.PublicKey);
                parent = parent.GetParent();
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

        private Guid ParseExpressionIdentifierToExistingQuestionIdResolvingThisIdentifierOrThrow(string identifier, Guid contextQuestionId, string expression)
        {
            if (IsSpecialThisIdentifier(identifier))
                return contextQuestionId;

            Guid parsedId;
            if (!Guid.TryParse(identifier, out parsedId))
                throw new QuestionnaireException(string.Format(
                    "Identifier '{0}' from expression '{1}' is not a 'this' keyword nor a valid guid.",
                    identifier, expression));

            this.ThrowIfThereAreNoCorrespondingQuestionsForExpressionIdentifierParsedToGuid(identifier, expression, parsedId);

            return parsedId;
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

        private bool IsPropogatableGroup(Guid groupId)
        {
            IGroup @group = this.GetGroupOrThrow(groupId);

            return @group.Propagated == Propagate.AutoPropagated;
        }

        private void ThrowIfGroupDoesNotExist(Guid groupId)
        {
            this.GetGroupOrThrow(groupId);
        }

        private bool HasGroup(Guid groupId)
        {
            return this.GetGroup(groupId) != null;
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
    }
}