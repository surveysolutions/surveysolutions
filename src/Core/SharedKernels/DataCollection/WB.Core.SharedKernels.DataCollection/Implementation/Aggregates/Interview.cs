using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class Interview : AggregateRootMappedByConvention
    {
        #region State

        private Guid questionnaireId;
        private long questionnaireVersion;

        private void Apply(InterviewCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        private void Apply(TextQuestionAnswered @event) {}

        private void Apply(NumericQuestionAnswered @event) {}

        private void Apply(DateTimeQuestionAnswered @event) {}

        private void Apply(SingleOptionQuestionAnswered @event) {}

        private void Apply(MultipleOptionsQuestionAnswered @event) {}

        #endregion

        #region Dependencies

        /// <summary>
        /// Repository which allows to get questionnaire.
        /// Should be used only in command handlers.
        /// And never in event handlers!!
        /// </summary>
        private IQuestionnaireRepository QuestionnaireRepository
        {
            get { return ServiceLocator.Current.GetInstance<IQuestionnaireRepository>(); }
        }

        #endregion

        /// <remarks>Is used to restore aggregate from event stream.</remarks>
        public Interview() {}

        public Interview(Guid questionnaireId, Guid userId)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(questionnaireId);

            this.ApplyEvent(new InterviewCreated(userId, questionnaireId, questionnaire.Version));
        }

        public void AnswerTextQuestion(Guid userId, Guid questionId, DateTime answerTime, string answer)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.Text);

            this.ApplyEvent(new TextQuestionAnswered(userId, questionId, answerTime, answer));
        }

        public void AnswerNumericQuestion(Guid userId, Guid questionId, DateTime answerTime, decimal answer)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.AutoPropagate, QuestionType.Numeric);

            this.ApplyEvent(new NumericQuestionAnswered(userId, questionId, answerTime, answer));
        }

        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, DateTime answerTime, DateTime answer)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.DateTime);

            this.ApplyEvent(new DateTimeQuestionAnswered(userId, questionId, answerTime, answer));
        }

        public void AnswerSingleOptionQuestion(Guid userId, Guid questionId, DateTime answerTime, decimal selectedValue)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.SingleOption);

            this.ApplyEvent(new SingleOptionQuestionAnswered(userId, questionId, answerTime, selectedValue));
        }

        public void AnswerMultipleOptionsQuestion(Guid userId, Guid questionId, DateTime answerTime, decimal[] selectedValues)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionnaire, questionId, QuestionType.MultyOption);

            this.ApplyEvent(new MultipleOptionsQuestionAnswered(userId, questionId, answerTime, selectedValues));
        }

        private IQuestionnaire GetHistoricalQuestionnaireOrThrow(Guid id, long version)
        {
            IQuestionnaire questionnaire = this.QuestionnaireRepository.GetHistoricalQuestionnaire(id, version);

            if (questionnaire == null)
                throw new InterviewException(string.Format("Questionnaire with id '{0}' of version {1} is not found.", id, version));

            return questionnaire;
        }

        private IQuestionnaire GetQuestionnaireOrThrow(Guid id)
        {
            IQuestionnaire questionnaire = this.QuestionnaireRepository.GetQuestionnaire(id);

            if (questionnaire == null)
                throw new InterviewException(string.Format("Questionnaire with id '{0}' is not found.", id));

            return questionnaire;
        }

        private static void ThrowIfQuestionTypeIsNotOneOfExpected(IQuestionnaire questionnaire, Guid questionId, params QuestionType[] expectedQuestionTypes)
        {
            QuestionType questionType = questionnaire.GetQuestionType(questionId);

            if (!expectedQuestionTypes.Contains(questionType))
                throw new InterviewException(string.Format(
                    "Question with id '{0}' has type {1}. But one of the following types was expected: {2}.",
                    questionId, questionType, string.Join(", ", expectedQuestionTypes.Select(type => type.ToString()))));
        }
    }
}
