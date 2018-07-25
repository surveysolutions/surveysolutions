using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    [TestFixture]
    internal class InterviewHistoryDenormalizer_when_recive_question_answer_event : InterviewHistoryDenormalizerTestContext
    {
        [Test]
        public void when_recive_DateTimeAnswer_for_currenttime_question_without_OriginDate()
        {
            var questionId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var variableName = "dt1";
            var answer = new DateTime(2017, 10, 19, 15, 46, 37);

            var interviewHistoryView = CreateInterviewHistoryView();
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Entity.DateTimeQuestion(questionId, isTimestamp: true)
            });
            var questionnaireStorage = Stub<IQuestionnaireStorage>.Returning(questionnaireDocument);
            var interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer(
                questionnaire: CreateQuestionnaireExportStructure(questionId, variableName),
                questionnaireStorage: questionnaireStorage);

            var dateTimeQuestionAnswered = Create.PublishedEvent.DateTimeQuestionAnswered(questionId: questionId, 
                answer: answer);
            dateTimeQuestionAnswered.Payload.OriginDate = null;

            // Act
            interviewExportedDataDenormalizer.Update(interviewHistoryView, dateTimeQuestionAnswered);

            // Assert
            var recordView = interviewHistoryView.Records[0];
            Assert.That(recordView.Action, Is.EqualTo(InterviewHistoricalAction.AnswerSet));
            Assert.That(recordView.Parameters["answer"], Is.EqualTo(answer.ToString(DateTimeFormat.DateWithTimeFormat)));
        }

        [Test]
        public void when_recive_DateTimeAnswer_for_currenttime_question_with_OriginDate()
        {
            var questionId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var variableName = "dt1";
            var answer = new DateTime(2017, 10, 19, 15, 46, 37);
            var answerOffset = TimeSpan.FromHours(2);
            var originDate = new DateTimeOffset(2017, 10, 19, 17, 46, 37, answerOffset);

            var interviewHistoryView = CreateInterviewHistoryView();
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Entity.DateTimeQuestion(questionId, isTimestamp: true)
            });
            var questionnaireStorage = Stub<IQuestionnaireStorage>.Returning(questionnaireDocument);
            var interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer(
                questionnaire: CreateQuestionnaireExportStructure(questionId, variableName),
                questionnaireStorage: questionnaireStorage);

            
            var dateTimeQuestionAnswered = Create.PublishedEvent.DateTimeQuestionAnswered(questionId: questionId,
                answer: answer,
                originDate: originDate);

            // Act
            interviewExportedDataDenormalizer.Update(interviewHistoryView, dateTimeQuestionAnswered);

            // Assert
            var recordView = interviewHistoryView.Records[0];
            Assert.That(recordView.Action, Is.EqualTo(InterviewHistoricalAction.AnswerSet));
            Assert.That(recordView.Offset, Is.EqualTo(answerOffset));
            Assert.That(recordView.Timestamp, Is.EqualTo(originDate.LocalDateTime), "Timestamp of the answer should always use server time when exporting");
            Assert.That(recordView.Parameters["answer"], Is.EqualTo(originDate.DateTime.ToString(DateTimeFormat.DateWithTimeFormat)));
            Assert.That(recordView.Parameters["answer"], Is.EqualTo(originDate.DateTime.ToString(DateTimeFormat.DateWithTimeFormat)));
        }

        [Test]
        public void when_recive_DateTimeAnswer_for_datetime_question()
        {
            var questionId = Guid.NewGuid();
            var variableName = "dt1";
            var answer = new DateTime(2017, 10, 19, 15, 46, 37);

            var interviewHistoryView = CreateInterviewHistoryView();
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new[]
            {
                Create.Entity.DateTimeQuestion(questionId, isTimestamp: false)
            });
            var questionnaireStorage = Stub<IQuestionnaireStorage>.Returning(questionnaireDocument);
            var interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer(
                questionnaire: CreateQuestionnaireExportStructure(questionId, variableName),
                questionnaireStorage: questionnaireStorage);
            var dateTimeQuestionAnswered = Create.PublishedEvent.DateTimeQuestionAnswered(questionId: questionId, answer: answer);


            interviewExportedDataDenormalizer.Update(interviewHistoryView, dateTimeQuestionAnswered);

            var recordView = interviewHistoryView.Records[0];
            Assert.AreEqual(recordView.Action, InterviewHistoricalAction.AnswerSet);
            Assert.AreEqual(recordView.Parameters["answer"], answer.ToString(DateTimeFormat.DateFormat));
        }
    }
}
