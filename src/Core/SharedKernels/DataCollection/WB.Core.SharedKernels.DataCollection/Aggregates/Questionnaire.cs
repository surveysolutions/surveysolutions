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
        private QuestionnaireDocument innerDocument = new QuestionnaireDocument();

        #region Dependencies

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

        protected internal void OnTemplateImported(TemplateImported e)
        {
            this.innerDocument = e.Source;
        }

        public IQuestion GetQuestionByStataCaption(string stataCaption)
        {
            return this.innerDocument.FirstOrDefault<IQuestion>(q => q.StataExportCaption == stataCaption);
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
                .Select(identifier => this.ParseExpressionIdentifierToExistingQuestionIdOrThrow(identifier, questionId, validationExpression))
                .ToList();
        }

        public string GetCustomValidationExpression(Guid questionId)
        {
            IQuestion question = this.GetQuestionOrThrow(questionId);

            return question.ValidationExpression;
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

        private Guid ParseExpressionIdentifierToExistingQuestionIdOrThrow(string identifier, Guid contextQuestionId, string expression)
        {
            if (IsSpecialThisIdentifier(identifier))
                return contextQuestionId;

            Guid parsedId;
            if (!Guid.TryParse(identifier, out parsedId))
                throw new QuestionnaireException(string.Format(
                    "Identifier '{0}' from expression '{1}' is not a 'this' keyword nor a valid guid.",
                    identifier, expression));

            if (!this.HasQuestionWithId(parsedId))
                throw new QuestionnaireException(string.Format(
                    "Identifier '{0}' from expression '{1}' is a valid guid '{2}' but questionnaire has no questions with such id.",
                    identifier, expression, parsedId));

            return parsedId;
        }

        private static bool IsSpecialThisIdentifier(string identifier)
        {
            return identifier.ToLower() == "this";
        }

        private static bool IsExpressionDefined(string expression)
        {
            return !string.IsNullOrWhiteSpace(expression);
        }

        private bool HasQuestionWithId(Guid questionId)
        {
            return this.GetQuestion(questionId) != null;
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
            return this.innerDocument.Find<IQuestion>(questionId);
        }
    }
}