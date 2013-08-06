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

namespace WB.Core.SharedKernels.DataCollection.Aggregates
{
    public class Questionnaire : AggregateRootMappedByConvention, IQuestionnaire
    {
        private QuestionnaireDocument innerDocument = new QuestionnaireDocument();

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

        private decimal ParseAnswerOptionValueOrThrow(string value, Guid questionId)
        {
            decimal parsedValue;

            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
                throw new QuestionnaireException(string.Format(
                    "Cannot parse answer option value '{0}' as decimal. Question id: '{1}'.",
                    value, questionId));

            return parsedValue;
        }

        private IQuestion GetQuestionOrThrow(Guid questionId)
        {
            var question = this.innerDocument.Find<IQuestion>(questionId);

            if (question == null)
                throw new QuestionnaireException(string.Format("Question with id '{0}' is not found.", questionId));

            return question;
        }
    }
}