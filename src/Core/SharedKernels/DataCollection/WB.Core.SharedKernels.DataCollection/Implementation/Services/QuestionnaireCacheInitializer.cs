using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    public class QuestionnaireCacheInitializer : IQuestionnaireCacheInitializer
    {
        private readonly IExpressionProcessor expressionProcessor;
        private readonly QuestionnaireDocument document;

        private Dictionary<Guid, IQuestion> questionCache = null;
        private Dictionary<Guid, IGroup> groupCache = null;

        public QuestionnaireCacheInitializer(QuestionnaireDocument document, IExpressionProcessor expressionProcessor)
        {
            this.document = document;
            this.expressionProcessor = expressionProcessor;
        }

        private Dictionary<Guid, IQuestion> QuestionCache
        {
            get
            {
                return this.questionCache ?? (this.questionCache
                    = this.document
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
                    = this.document
                        .Find<IGroup>(_ => true)
                        .ToDictionary(
                            group => group.PublicKey,
                            group => group));
            }
        }

        public void WarmUpCaches()
        {
            if(this.document.IsCacheWarmed)
                return;

            var questionWarmingUpMethods = new Action<Guid>[]
            {
                questionId => this.GetQuestionsInvolvedInCustomValidation(questionId),
                questionId => this.GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(questionId),
                questionId => this.GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(questionId),
                questionId => this.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionId),
                questionId => this.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(questionId)
            };

            foreach (IGroup group in this.GroupCache.Values)
            {
                try
                {
                    this.GetQuestionsInvolvedInCustomEnablementConditionOfGroup(group.PublicKey);
                }
                catch { }
            }

            foreach (Action<Guid> method in questionWarmingUpMethods)
            {
                foreach (IQuestion question in this.QuestionCache.Values)
                {

                    try
                    {
                        method.Invoke(question.PublicKey);
                    }
                    catch {}
                }
            }

            this.document.IsCacheWarmed = true;
        }

        private void GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            question.QuestionIdsInvolvedInCustomEnablementConditionOfQuestion = this.GetQuestionsInvolvedInExpression(question.PublicKey, question.ConditionExpression).ToList();
        }

        private void GetQuestionsInvolvedInCustomValidation(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);
            question.QuestionIdsInvolvedInCustomValidationOfQuestion = this.GetQuestionsInvolvedInExpression(question.PublicKey, question.ValidationExpression).ToList();
        }

        private void GetQuestionsInvolvedInCustomEnablementConditionOfGroup(Guid questionId)
        {
            IGroup group = this.GetGroup(questionId);
            group.QuestionIdsInvolvedInCustomEnablementConditionOfGroup = this.GetQuestionsInvolvedInExpression(group.PublicKey, group.ConditionExpression).ToList();
        }

        private void GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(Guid questionId)
        {
            var targetQuestion = this.GetQuestion(questionId);
            targetQuestion.QuestionsWhichCustomValidationDependsOnQuestion = Enumerable.ToList(
                from question in this.QuestionCache.Values
                where this.DoesQuestionCustomValidationDependOnSpecifiedQuestion(question.PublicKey, specifiedQuestionId: questionId)
                    && questionId != question.PublicKey
                select question.PublicKey
                );
        }

        private void GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId)
        {
            var targetQuestion = this.GetQuestion(questionId);
            targetQuestion.QuestionsWithDependentEnablementConditions = Enumerable.ToList(
                from question in this.QuestionCache.Values
                where this.DoesQuestionCustomEnablementDependOnSpecifiedQuestion(question.PublicKey, specifiedQuestionId: questionId)
                    && questionId != question.PublicKey
                select question.PublicKey
                );
        }

        private void GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(Guid questionId)
        {
            var targetQuestion = this.GetQuestion(questionId);
            targetQuestion.GroupsWithDependentEnablementConditions = Enumerable.ToList(
                from @group in this.GroupCache.Values
                where this.DoesGroupCustomEnablementDependOnSpecifiedQuestion(@group.PublicKey, specifiedQuestionId: questionId)
                select @group.PublicKey
                );
        }

        private IEnumerable<Guid> GetQuestionsInvolvedInExpression(Guid contextQuestionId, string expression)
        {
            if (!IsExpressionDefined(expression))
                return Enumerable.Empty<Guid>();

            IEnumerable<string> identifiersUsedInExpression = this.expressionProcessor.GetIdentifiersUsedInExpression(expression);

            return this.DistinctlyResolveExpressionIdentifiersToExistingQuestionIdsReplacingThisIdentifierOrThrow(
                identifiersUsedInExpression, contextQuestionId, expression);
        }

        private IQuestion GetQuestionOrThrow(Guid questionId)
        {
            IQuestion question = this.GetQuestion(questionId);

            if (question == null)
                throw new NullReferenceException(string.Format("Question with id '{0}' is not found.", questionId));

            return question;
        }

        private static bool IsExpressionDefined(string expression)
        {
            return !string.IsNullOrWhiteSpace(expression);
        }

        private IEnumerable<Guid> DistinctlyResolveExpressionIdentifiersToExistingQuestionIdsReplacingThisIdentifierOrThrow(
            IEnumerable<string> identifiers, Guid contextQuestionId, string expression)
        {
            var distinctQuestionIds = new HashSet<Guid>();

            foreach (var identifier in identifiers)
            {
                if (IsSpecialThisIdentifier(identifier))
                {
                    distinctQuestionIds.Add(contextQuestionId);
                }
                else
                {
                    distinctQuestionIds.Add(this.ParseExpressionIdentifierToExistingQuestionIdIgnoringThisIdentifierOrThrow(identifier,
                        expression));
                }
            }

            return distinctQuestionIds;
        }

        private static bool IsSpecialThisIdentifier(string identifier)
        {
            return identifier.ToLower() == "this";
        }

        private Guid ParseExpressionIdentifierToExistingQuestionIdIgnoringThisIdentifierOrThrow(string identifier,
            string expression)
        {
            IQuestion question = this.GetQuestionByStringIdOrVariableName(identifier);

            if (question == null)
                throw new NullReferenceException(string.Format(
                    "Identifier '{0}' from expression '{1}' is not valid question identifier. Question with such a identifier is missing.",
                    identifier, expression));

            return question.PublicKey;
        }

        private IQuestion GetQuestionByStringIdOrVariableName(string identifier)
        {
            Guid parsedId;
            return !Guid.TryParse(identifier, out parsedId) ? this.GetQuestionByStataCaption(identifier) : this.GetQuestion(parsedId);
        }

        private IQuestion GetQuestion(Guid questionId)
        {
            return this.QuestionCache.ContainsKey(questionId)
                ? this.QuestionCache[questionId]
                : null;
        }

        private IGroup GetGroup(Guid groupId)
        {
            return this.GroupCache.ContainsKey(groupId)
                ? this.GroupCache[groupId]
                : null;
        }

        private IQuestion GetQuestionByStataCaption(string identifier)
        {
            return this.document.FirstOrDefault<IQuestion>(q => q.StataExportCaption == identifier);
        }

        private bool DoesQuestionCustomValidationDependOnSpecifiedQuestion(Guid questionId, Guid specifiedQuestionId)
        {
            var question = this.GetQuestion(questionId);

            IEnumerable<Guid> involvedQuestions = question.QuestionIdsInvolvedInCustomValidationOfQuestion;

            bool isSpecifiedQuestionInvolved = involvedQuestions.Contains(specifiedQuestionId);

            return isSpecifiedQuestionInvolved;
        }

        private bool DoesQuestionCustomEnablementDependOnSpecifiedQuestion(Guid questionId, Guid specifiedQuestionId)
        {
            var question = this.GetQuestion(questionId);

            IEnumerable<Guid> involvedQuestions = question.QuestionIdsInvolvedInCustomEnablementConditionOfQuestion;

            bool isSpecifiedQuestionInvolved = involvedQuestions.Contains(specifiedQuestionId);

            return isSpecifiedQuestionInvolved;
        }

        private bool DoesGroupCustomEnablementDependOnSpecifiedQuestion(Guid groupId, Guid specifiedQuestionId)
        {
            var group = this.GetGroup(groupId);

            IEnumerable<Guid> involvedQuestions = group.QuestionIdsInvolvedInCustomEnablementConditionOfGroup;

            bool isSpecifiedQuestionInvolved = involvedQuestions.Contains(specifiedQuestionId);

            return isSpecifiedQuestionInvolved;
        }
    }
}
